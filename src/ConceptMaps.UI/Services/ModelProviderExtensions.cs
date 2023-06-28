namespace ConceptMaps.UI.Services;

public static class ModelProviderExtensions
{
    public static bool IsActiveModel(this IModelProvider provider, ModelType modelType, string modelName)
    {
        if (modelType == ModelType.Nrt)
        {
            return provider.ActiveNrtModel == modelName;
        }
        
        if (modelType == ModelType.Relation)
        {
            return provider.ActiveRelationsModel == modelName;
        }

        throw new ArgumentOutOfRangeException(nameof(modelType));
    }
}