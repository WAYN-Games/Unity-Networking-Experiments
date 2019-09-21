using Unity.Mathematics;
using Unity.Networking.Transport;

public struct CubeSnapshotData : ISnapshotData<CubeSnapshotData>
{
    public uint tick;
    int RotationValueX;
    int RotationValueY;
    int RotationValueZ;
    int RotationValueW;
    int TranslationValueX;
    int TranslationValueY;
    int TranslationValueZ;


    public uint Tick => tick;
    public quaternion GetRotationValue()
    {
        return new quaternion(RotationValueX * 0.001f, RotationValueY * 0.001f, RotationValueZ * 0.001f, RotationValueW * 0.001f);
    }
    public void SetRotationValue(quaternion q)
    {
        RotationValueX = (int)(q.value.x * 1000);
        RotationValueY = (int)(q.value.y * 1000);
        RotationValueZ = (int)(q.value.z * 1000);
        RotationValueW = (int)(q.value.w * 1000);
    }
    public float3 GetTranslationValue()
    {
        return new float3(TranslationValueX, TranslationValueY, TranslationValueZ) * 0.1f;
    }
    public void SetTranslationValue(float3 val)
    {
        TranslationValueX = (int)(val.x * 10);
        TranslationValueY = (int)(val.y * 10);
        TranslationValueZ = (int)(val.z * 10);
    }


    public void PredictDelta(uint tick, ref CubeSnapshotData baseline1, ref CubeSnapshotData baseline2)
    {
        var predictor = new GhostDeltaPredictor(tick, this.tick, baseline1.tick, baseline2.tick);
        RotationValueX = predictor.PredictInt(RotationValueX, baseline1.RotationValueX, baseline2.RotationValueX);
        RotationValueY = predictor.PredictInt(RotationValueY, baseline1.RotationValueY, baseline2.RotationValueY);
        RotationValueZ = predictor.PredictInt(RotationValueZ, baseline1.RotationValueZ, baseline2.RotationValueZ);
        RotationValueW = predictor.PredictInt(RotationValueW, baseline1.RotationValueW, baseline2.RotationValueW);
        TranslationValueX = predictor.PredictInt(TranslationValueX, baseline1.TranslationValueX, baseline2.TranslationValueX);
        TranslationValueY = predictor.PredictInt(TranslationValueY, baseline1.TranslationValueY, baseline2.TranslationValueY);
        TranslationValueZ = predictor.PredictInt(TranslationValueZ, baseline1.TranslationValueZ, baseline2.TranslationValueZ);

    }

    public void Serialize(ref CubeSnapshotData baseline, DataStreamWriter writer, NetworkCompressionModel compressionModel)
    {
        writer.WritePackedIntDelta(RotationValueX, baseline.RotationValueX, compressionModel);
        writer.WritePackedIntDelta(RotationValueY, baseline.RotationValueY, compressionModel);
        writer.WritePackedIntDelta(RotationValueZ, baseline.RotationValueZ, compressionModel);
        writer.WritePackedIntDelta(RotationValueW, baseline.RotationValueW, compressionModel);
        writer.WritePackedIntDelta(TranslationValueX, baseline.TranslationValueX, compressionModel);
        writer.WritePackedIntDelta(TranslationValueY, baseline.TranslationValueY, compressionModel);
        writer.WritePackedIntDelta(TranslationValueZ, baseline.TranslationValueZ, compressionModel);

    }

    public void Deserialize(uint tick, ref CubeSnapshotData baseline, DataStreamReader reader, ref DataStreamReader.Context ctx,
        NetworkCompressionModel compressionModel)
    {
        this.tick = tick;
        RotationValueX = reader.ReadPackedIntDelta(ref ctx, baseline.RotationValueX, compressionModel);
        RotationValueY = reader.ReadPackedIntDelta(ref ctx, baseline.RotationValueY, compressionModel);
        RotationValueZ = reader.ReadPackedIntDelta(ref ctx, baseline.RotationValueZ, compressionModel);
        RotationValueW = reader.ReadPackedIntDelta(ref ctx, baseline.RotationValueW, compressionModel);
        TranslationValueX = reader.ReadPackedIntDelta(ref ctx, baseline.TranslationValueX, compressionModel);
        TranslationValueY = reader.ReadPackedIntDelta(ref ctx, baseline.TranslationValueY, compressionModel);
        TranslationValueZ = reader.ReadPackedIntDelta(ref ctx, baseline.TranslationValueZ, compressionModel);

    }
    public void Interpolate(ref CubeSnapshotData target, float factor)
    {
        SetRotationValue(math.slerp(GetRotationValue(), target.GetRotationValue(), factor));
        SetTranslationValue(math.lerp(GetTranslationValue(), target.GetTranslationValue(), factor));

    }
}