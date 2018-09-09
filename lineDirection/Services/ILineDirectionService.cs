namespace LineDirection.Services
{
    interface ILineDirectionService
    {
        bool ProcessLinesInFile(string selectedFile, OutputType outputType);

        string WriteCSV(string path);
    }
}
