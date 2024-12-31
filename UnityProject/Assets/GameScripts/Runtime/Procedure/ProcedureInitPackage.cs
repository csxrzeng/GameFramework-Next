﻿using System;
using Cysharp.Threading.Tasks;
using GameFramework;
using UnityGameFramework.Runtime;
using YooAsset;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace GameMain
{
    /// <summary>
    /// 流程 => 初始化Package。
    /// </summary>
    public class ProcedureInitPackage : ProcedureBase
    {
        public override bool UseNativeDialog { get; }

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            
            //Fire Forget立刻触发UniTask初始化Package
            InitPackage(procedureOwner).Forget();
        }

        private async UniTaskVoid InitPackage(ProcedureOwner procedureOwner)
        {
            await UniTask.WaitForSeconds(0.1f);
            try
            {
                var package = YooAssets.TryGetPackage(GameModule.Resource.PackageName);
                if (package != null && package.InitializeStatus == EOperationStatus.Succeed)
                {
                    OnInitSuccess(procedureOwner);
                    return;
                }
                var initializationOperation = await GameModule.Resource.InitPackage();

                if (initializationOperation.Status == EOperationStatus.Succeed)
                {
                    OnInitSuccess(procedureOwner);
                }
                else
                {
                    OnInitPackageFailed(procedureOwner, initializationOperation.Error);
                }
            }
            catch (Exception e)
            {
                OnInitPackageFailed(procedureOwner, e.Message);
            }
        }

        private void OnInitSuccess(ProcedureOwner procedureOwner)
        {
            // 编辑器模式。
            if (GameModule.Resource.PlayMode == EPlayMode.EditorSimulateMode)
            {
                Log.Info("Editor resource mode detected.");
                ChangeState<ProcedurePreload>(procedureOwner);
            }
            // 单机模式。
            else if (GameModule.Resource.PlayMode == EPlayMode.OfflinePlayMode)
            {
                Log.Info("Package resource mode detected.");
                ChangeState<ProcedureInitResources>(procedureOwner);
            }
            // 可更新模式。
            else if (GameModule.Resource.PlayMode == EPlayMode.HostPlayMode ||
                     GameModule.Resource.PlayMode == EPlayMode.WebPlayMode)
            {
                // 打开启动UI。
                UILoadMgr.Show(UIDefine.UILoadUpdate);

                Log.Info("Updatable resource mode detected.");
                ChangeState<ProcedureUpdateVersion>(procedureOwner);
            }
            else
            {
                Log.Error("UnKnow resource mode detected Please check???");
            }
        }

        private void OnInitPackageFailed(ProcedureOwner procedureOwner, string message)
        {
            // 打开启动UI。
            UILoadMgr.Show(UIDefine.UILoadUpdate);

            Log.Error($"{message}");

            // 打开启动UI。
            UILoadMgr.Show(UIDefine.UILoadUpdate, LoadText.Instance.Label_Load_Init);

            UILoadTip.ShowMessageBox(Utility.Text.Format(LoadText.Instance.Label_Init_Failed, message), MessageShowType.TwoButton,
                LoadStyle.StyleEnum.Style_Retry
                , () => { Retry(procedureOwner); }, 
                GameModule.QuitApplication);
        }

        private void Retry(ProcedureOwner procedureOwner)
        {
            // 打开启动UI。
            UILoadMgr.Show(UIDefine.UILoadUpdate, LoadText.Instance.Label_Load_InitRetry);

            InitPackage(procedureOwner).Forget();
        }
    }
}