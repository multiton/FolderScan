using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace DiskWatcher
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void RaisePropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            if (propertyExpression == null || propertyExpression.Body.NodeType != ExpressionType.MemberAccess)
            {
                return;
            }

            var memberExpr = propertyExpression.Body as MemberExpression;

            if (memberExpr != null)
            {
                RaisePropertyChanged(memberExpr.Member.Name);
            }
        }
    }
}