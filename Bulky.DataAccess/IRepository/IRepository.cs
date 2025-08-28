using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.IRepository
{
    public interface IRepository<T> where T:class
    {

        IEnumerable<T> GetAll(string? include =null);
        T Get(Expression<Func<T,bool>> filter, string? include = null);

        void Add(T Entity);
        //void Update(T Entity);
        void Remove(T Entity);
        void RemoveRange(IEnumerable<T> Entity);


    }
}
