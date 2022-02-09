using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLuceneNET
{
    public static class EasyLuceneNetExtensions
    {
        public static IServiceCollection AddEasyLuceneNet(this IServiceCollection service)
        {
            service.AddSingleton<IEasyLuceneNet, EasyLuceneNetDefaultProvider>();
            return service;
        }
    }
}
