using System.Collections.Generic;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Manifest;
using UmbracoLitExtension.Extension;

namespace NodeNameSync.Composing
{
    public class NodeNameSyncComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.ManifestFilters().Append<Manifest>();
        }
    }

    internal class Manifest : IManifestFilter
    {
        public void Filter(List<PackageManifest> manifests)
        {
            manifests.Add(new PackageManifest
            {
                PackageName = "Infocaster.Umbraco.NodeNameSync",
                BundleOptions = BundleOptions.None,
                Scripts = new[]
                {
                    Defaults.PluginBasePath + "/script.iife.js"
                },
            });
        }
    }
}
