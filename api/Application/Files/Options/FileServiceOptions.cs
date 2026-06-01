namespace Application.Files.Options;

public class FileServiceOptions
{
    public const string SectionName = "FileService";

    public string BaseUrl { get; set; } = "http://file-service";
}
