using System.Collections.Generic;
using System.Linq;
using Submodules.Common.Tools.SubclassSelector;
using UnityEngine;

namespace RubbishPot.Core
{
    [System.Serializable]
    public class CheckingTree : IOrderRequest
    {
        [SubClassSelector] [SerializeReference] private ICheckingLogic _logic;

        private CheckingTree(ICheckingLogic logic)
        {
            _logic = logic;
        }
        
        public bool Check(Potion potion)
        {
            return _logic.Check(potion);
        }

        #region Logics

        private interface ICheckingLogic
        {
            public bool Check(Potion potion);
        }
        
        [System.Serializable]
        public class And : ICheckingLogic
        {
            [SubClassSelector] [SerializeReference] private List<IOrderRequest> _requests;

            public bool Check(Potion potion)
            {
                return _requests.All(r => r.Check(potion));
            }
        }
        
        [System.Serializable]
        public class Or : ICheckingLogic
        {
            [SubClassSelector] [SerializeReference] private List<IOrderRequest> _requests;
            
            public bool Check(Potion potion)
            {
                return _requests.Any(r => r.Check(potion));
            }
        }

        #endregion
    }
}
