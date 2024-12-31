using Cysharp.Threading.Tasks;
using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace GameMain
{
    /// <summary>
    /// 流程 => 清理缓存。
    /// </summary>
    public class ProcedureClearCache:ProcedureBase
    {
        public override bool UseNativeDialog { get; }

        private ProcedureOwner _procedureOwner;
        
        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            _procedureOwner = procedureOwner;
            Log.Info(LoadText.Instance.Label_ClearCache);
            
            UILoadMgr.Show(UIDefine.UILoadUpdate, LoadText.Instance.Label_ClearCache);
            
            var operation = GameModule.Resource.ClearUnusedCacheFilesAsync();
            operation.Completed += Operation_Completed;
        }
        
        
        private void Operation_Completed(YooAsset.AsyncOperationBase obj)
        {
            UILoadMgr.Show(UIDefine.UILoadUpdate, LoadText.Instance.Label_ClearCacheDone);
            
            ChangeState<ProcedureLoadAssembly>(_procedureOwner);
        }
    }
}