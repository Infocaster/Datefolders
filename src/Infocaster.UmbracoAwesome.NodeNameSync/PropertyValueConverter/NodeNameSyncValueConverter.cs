using System;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;

namespace NodeNameSync.PropertyValueConverter;

public class NodeNameSyncValueConverter : PropertyValueConverterBase
{
    public override bool IsConverter(IPublishedPropertyType propertyType)
    {
        return propertyType.EditorAlias is "Infocaster.NodeNameSync";
    }
    
public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
    {
        return typeof(string);
    }
    
public override object? ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
    {
        return source?.ToString();
    }
    
public override object? ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview)
    {
        // Node name sync is a simple string.
        // Conversion has already been done in the source-to-intermediate step
        return inter;
    }
}
