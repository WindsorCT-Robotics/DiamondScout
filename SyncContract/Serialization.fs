namespace ParagonRobotics.DiamondScout.SyncContract

open System.Text.Json
open ZstdNet

type SerializationError =
    | DataTooLarge of data: byte array

type Serializer =
    static member Serialize data =
        use compressorOptions = new CompressionOptions(CompressionOptions.MaxCompressionLevel)
        use compressor = new Compressor(compressorOptions)
        let jsonOptions = JsonSerializerOptions()
        jsonOptions.WriteIndented <- false

        JsonSerializer.SerializeToUtf8Bytes(data, jsonOptions)
        |> compressor.Wrap
        |> function
            | data when data.Length > 2953 -> data |> DataTooLarge |> Error
            | data -> data |> Ok
    static member Deserialize<'T> (data: byte array) =
        use decompressor = new Decompressor()

        data
        |> decompressor.Unwrap
        |> JsonSerializer.Deserialize<'T>
    
