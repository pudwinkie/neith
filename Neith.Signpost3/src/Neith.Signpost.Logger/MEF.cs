using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Neith.Util.Reflection;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;

namespace Neith.Signpost.Logger
{
    internal static class MEF
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static CompositionContainer Compose()
        {
            var catalog = new AggregateCatalog();
            foreach (var cat in EnCatalog()) catalog.Catalogs.Add(cat);
            var container = new CompositionContainer(catalog);

            // 起動時初期化ルーチンを一斉呼び出し
            foreach (var init in container.GetExportedValues<IMEFInitializer>()) {
                init.Compose(container);
            }

            return container;
        }
        private static IEnumerable<ComposablePartCatalog> EnCatalog()
        {
            yield return new DirectoryCatalog(AssemblyUtil.GetCallingAssemblyDirctory(), "Neith.Logger*.dll");
            yield return new DirectoryCatalog(AssemblyUtil.GetCallingAssemblyDirctory(), "Neith.Signpost*.dll");
        }
    }

    public interface IMEFInitializer
    {
        /// <summary>
        /// 初期化関数。MEF::Composeより呼び出され、初期化が行われます。
        /// </summary>
        /// <param name="container"></param>
        void Compose(CompositionContainer container);
    }
}