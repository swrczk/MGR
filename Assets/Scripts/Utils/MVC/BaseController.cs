using System;
using Unity.VisualScripting;
using UnityEngine;

namespace Utils.MVC
{
    public abstract class BaseController<TView, TContext> : BaseController
        where TView : BaseView
        where TContext : BaseContext
    {
        protected TView View  ;
        protected TContext Context { get; private set; }

        public override Type GetViewType()
        {
            return typeof(TView);
        }

        public override void AssignView(IView baseView)
        {
            View = baseView as TView;
            Context = this.GameObject().AddComponent<TContext>();
            Context.ResolveContext();
        }

        public override void Setup()
        {
            CreateDispatcher();
            View.Setup();
        }
        protected virtual void CreateDispatcher(){}
    }

    public abstract class BaseController : MonoBehaviour, IController
    {
        public abstract void Setup();
        public abstract Type GetViewType();
        public abstract void AssignView(IView baseView);
    }

    public interface IController
    {
        public void Setup();
        public Type GetViewType();
        public void AssignView(IView baseView);
    }
}