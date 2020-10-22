
namespace Wrld.Resources.IndoorMaps
{
    /// <summary>
    /// This interface is used to create IIndoorMapMaterials from IndoorMaterialDescriptors; implement it in your own classes to provide customised material creation for Indoor Maps.
    /// A default implementation exists in DefaultIndoorMapMaterialFactory.cs.
    /// </summary>
    public interface IIndoorMapMaterialFactory
    {
        /// <summary>
        /// Implement this function with logic to produce an IIndoorMapMaterial from the given IndoorMaterialDescriptor. It is called when materials are streamed in for an Indoor Map.
        /// </summary>
        /// <param name="descriptor">The IndoorMaterialDescriptor object which contains a number of material parameters used to build this material.</param>
        /// <returns>The IIndoorMapMaterial that you have built.</returns>
        IIndoorMapMaterial CreateMaterialFromDescriptor(IndoorMaterialDescriptor descriptor);
    }
}