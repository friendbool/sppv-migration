
namespace SPPersonalViewMigrate
{
    internal struct Usage
    {
        public const string Export = "SPPersonalViewMigrate.exe -o export \r\n    -url <site colletion or site url> \r\n    -file <absolute path to the export file>";
        public const string Import = "SPPersonalViewMigrate.exe -o import \r\n    -file <absolute path to the export file> \r\n    -source <site relative url> \r\n    -target <absolute url> \r\n    [-view <view name>] \r\n    [-login <login name>] \r\n    [-excludeLogin <login names to exlcude separated by comma>] \r\n    [-schemaPlainText]";
    }
}
