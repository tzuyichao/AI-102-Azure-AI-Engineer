﻿using Microsoft.Extensions.Configuration;
using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;

namespace layout_model
{
    class Program
    {
        static DocumentAnalysisClient daClient;

        static async Task Main() 
        {
            try
            {
                IConfigurationBuilder builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
                IConfigurationRoot configuration = builder.Build();
                string endpoint = configuration["CognitiveServiceEndpoint"];
                string key = configuration["CognitiveServiceKey"];

                // create client
                AzureKeyCredential credential = new AzureKeyCredential(key);
                daClient = new DocumentAnalysisClient(new Uri(endpoint), credential);

                // sample document from miccrosoft
                Uri fileUri = new Uri("https://raw.githubusercontent.com/Azure-Samples/cognitive-services-REST-api-samples/master/curl/form-recognizer/sample-layout.pdf");
                AnalyzeDocumentOperation operation = await daClient.AnalyzeDocumentFromUriAsync(WaitUntil.Completed, "prebuilt-layout", fileUri);
                AnalyzeResult result = operation.Value;

                // Pages  
                foreach(DocumentPage page in result.Pages)
                {
                    Console.WriteLine($"Document Page {page.PageNumber} has {page.Lines.Count} line(s), {page.Words.Count} word(s),");
                    Console.WriteLine($" and {page.SelectionMarks.Count} selection mark(s).");

                    // lines
                    for(int i=0; i<page.Lines.Count; i++)
                    {
                        DocumentLine line = page.Lines[i];
                        Console.WriteLine($"  Line {i} has content: '{line.Content}'");
                        Console.WriteLine($"    Its bounding box is:");
                        Console.WriteLine($"      Upper left => X: {line.BoundingPolygon[0].X}, Y= {line.BoundingPolygon[0].Y}");
                        Console.WriteLine($"      Upper right => X: {line.BoundingPolygon[1].X}, Y= {line.BoundingPolygon[1].Y}");
                        Console.WriteLine($"      Lower right => X: {line.BoundingPolygon[2].X}, Y= {line.BoundingPolygon[2].Y}");
                        Console.WriteLine($"      Lower left => X: {line.BoundingPolygon[3].X}, Y= {line.BoundingPolygon[3].Y}");
                    }

                    // selection marks
                    for(int i=0; i<page.SelectionMarks.Count; i++)
                    {
                        DocumentSelectionMark selectionMark = page.SelectionMarks[i];
                        Console.WriteLine($"  Selection Mark {i} is {selectionMark.State}.");
                        Console.WriteLine($"    Its bounding box is:");
                        Console.WriteLine($"      Upper left => X: {selectionMark.BoundingPolygon[0].X}, Y= {selectionMark.BoundingPolygon[0].Y}");
                        Console.WriteLine($"      Upper right => X: {selectionMark.BoundingPolygon[1].X}, Y= {selectionMark.BoundingPolygon[1].Y}");
                        Console.WriteLine($"      Lower right => X: {selectionMark.BoundingPolygon[2].X}, Y= {selectionMark.BoundingPolygon[2].Y}");
                        Console.WriteLine($"      Lower left => X: {selectionMark.BoundingPolygon[3].X}, Y= {selectionMark.BoundingPolygon[3].Y}");                        
                    }
                }
                
                // Styles
                foreach(DocumentStyle style in result.Styles)
                {
                    bool isHandwritten = style.IsHandwritten.HasValue && style.IsHandwritten == true;
                    
                    if(isHandwritten && style.Confidence > 0.8)
                    {
                        Console.WriteLine($"Handwritten content found:");

                        foreach(DocumentSpan span in style.Spans)
                        {
                            Console.WriteLine($"  Content: {result.Content.Substring(span.Index, span.Length)}");
                        }
                    }
                }

                // Tables
                Console.WriteLine("The following tables were extracted:");
                for(int i=0; i<result.Tables.Count; i++)
                {
                    DocumentTable table = result.Tables[i];
                    Console.WriteLine($"  Table {i} has {table.RowCount} rows and {table.ColumnCount} columns.");
                    foreach(DocumentTableCell cell in table.Cells)
                    {
                        Console.WriteLine($"    Cell ({cell.RowIndex}, {cell.ColumnIndex}) has kind '{cell.Kind}' and content: '{cell.Content}'");
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}