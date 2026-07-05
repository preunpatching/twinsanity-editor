using System.IO;

namespace Twinsanity
{
    public class RigidModel : TwinsItem
    {
        public uint Header { get; set; } = 257;
        public uint[] MaterialIDs { get; set; } = new uint[1] { 0 };
        public uint MeshID { get; set; }

        public override void Save(BinaryWriter writer)
        {
            writer.Write(Header);
            writer.Write(MaterialIDs.Length);
            for (int i = 0; i < MaterialIDs.Length; ++i)
            {
                writer.Write(MaterialIDs[i]);
            }

            writer.Write(MeshID);
        }

        public override void Load(BinaryReader reader, int size)
        {
            Header = reader.ReadUInt32();
            int count = reader.ReadInt32();
            MaterialIDs = new uint[count];
            for (int i = 0; i < count; ++i)
            {
                MaterialIDs[i] = reader.ReadUInt32();
            }

            MeshID = reader.ReadUInt32();
        }

        protected override int GetSize()
        {
            return 12 + (MaterialIDs.Length * 4);
        }

        internal void FillPackage(TwinsFile source, TwinsFile destination)
        {
            TwinsSection sourceMaterials = source.GetItem<TwinsSection>(11).GetItem<TwinsSection>(1);
            TwinsSection destinationMaterials = destination.GetItem<TwinsSection>(11).GetItem<TwinsSection>(1);
            TwinsSection sourceMeshes = source.GetItem<TwinsSection>(11).GetItem<TwinsSection>(2);
            TwinsSection destinationMeshes = destination.GetItem<TwinsSection>(11).GetItem<TwinsSection>(2);
            foreach (uint materialId in MaterialIDs)
            {
                if (destinationMaterials.HasItem(materialId))
                {
                    continue;
                }
                Material linkedMaterial = sourceMaterials.GetItem<Material>(materialId);
                destinationMaterials.AddItem(materialId, linkedMaterial);
                linkedMaterial.FillPackage(source, destination);
            }
            if (!destinationMeshes.HasItem(MeshID))
            {
                Model linkedMesh = sourceMeshes.GetItem<Model>(MeshID);
                destinationMeshes.AddItem(MeshID, linkedMesh);
            }
        }

        internal void FillPackageXbox(TwinsFile source, TwinsFile destination)
        {
            TwinsSection sourceMaterials = source.GetItem<TwinsSection>(11).GetItem<TwinsSection>(1);
            TwinsSection destinationMaterials = destination.GetItem<TwinsSection>(11).GetItem<TwinsSection>(1);
            TwinsSection sourceMeshes = source.GetItem<TwinsSection>(11).GetItem<TwinsSection>(2);
            TwinsSection destinationMeshes = destination.GetItem<TwinsSection>(11).GetItem<TwinsSection>(2);
            foreach (uint materialId in MaterialIDs)
            {
                if (destinationMaterials.HasItem(materialId))
                {
                    continue;
                }
                Material linkedMaterial = sourceMaterials.GetItem<Material>(materialId);
                destinationMaterials.AddItem(materialId, linkedMaterial);
                linkedMaterial.FillPackageXbox(source, destination);
            }
            if (!destinationMeshes.HasItem(MeshID))
            {
                ModelX linkedMesh = sourceMeshes.GetItem<ModelX>(MeshID);
                destinationMeshes.AddItem(MeshID, linkedMesh);
            }
        }

        internal void FillPackageDemo(TwinsFile source, TwinsFile destination)
        {
            TwinsSection sourceMaterials = source.GetItem<TwinsSection>(11).GetItem<TwinsSection>(1);
            TwinsSection destinationMaterials = destination.GetItem<TwinsSection>(11).GetItem<TwinsSection>(1);
            TwinsSection sourceMeshes = source.GetItem<TwinsSection>(11).GetItem<TwinsSection>(2);
            TwinsSection destinationMeshes = destination.GetItem<TwinsSection>(11).GetItem<TwinsSection>(2);
            foreach (uint materialId in MaterialIDs)
            {
                if (destinationMaterials.HasItem(materialId))
                {
                    continue;
                }
                MaterialDemo linkedMaterial = sourceMaterials.GetItem<MaterialDemo>(materialId);
                destinationMaterials.AddItem(materialId, linkedMaterial);
                linkedMaterial.FillPackage(source, destination);
            }
            if (!destinationMeshes.HasItem(MeshID))
            {
                Model linkedMesh = sourceMeshes.GetItem<Model>(MeshID);
                destinationMeshes.AddItem(MeshID, linkedMesh);
            }
        }
    }
}
