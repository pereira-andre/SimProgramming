using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms;
using Microsoft.ML.Transforms.Text;
using Microsoft.ML.FastTree;

namespace SharpCEstate
{
    public static class Configurations
    {
        public static void Load()
        {
            Console.WriteLine("Carregando configurações...");
        }
    }

    public static class ModelInitializer
    {
        public static ITransformer InitializeModel(MLContext mlContext)
        {
            Console.WriteLine("Inicializando modelo de Machine Learning...");
            try
            {
                var modelPath = "model.zip";
                DataViewSchema modelSchema;
                return mlContext.Model.Load(modelPath, out modelSchema);
            }
            catch (Exception)
            {
                Console.WriteLine("Modelo não encontrado, um novo modelo padrão será treinado.");
                return CreateDefaultModel(mlContext); // Cria e retorna um modelo padrão se falhar
            }
        }

        private static ITransformer CreateDefaultModel(MLContext mlContext)
        {
            // Assume ProcessedRealEstateData includes the proper properties, 
            // and has fields that will be used as features.
            var data = new List<ProcessedRealEstateData>
            {
                new ProcessedRealEstateData { Nome = "Example", PrecoFloat = 0.0f, AreaFloat = 0.0f, Localizacao = "None" }
            };

            // Load the data into an IDataView.
            var dataView = mlContext.Data.LoadFromEnumerable(data);

            // Set up the pipeline to create the 'Features' column
            var pipeline = mlContext.Transforms.Text.FeaturizeText(outputColumnName: "DescriptionFeaturized", inputColumnName: "Nome")
                .Append(mlContext.Transforms.Concatenate("Features", "DescriptionFeaturized", "AreaFloat"))  // Here, concatenate 'Area' with 'Preco' to form 'Features'
                .Append(mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: "PrecoFloat"))  // Ensure 'Label' column is created from 'Preco'
                .Append(mlContext.Regression.Trainers.FastTree());

            return pipeline.Fit(dataView);
        }
    }

    public static class RealEstateDataProcessor
    {
        public static IDataView LoadAndPrepareData(MLContext mlContext, string filePath)
        {
            filePath = @"C:\Users\lucia\SimProgramming\SharpCEstate\imovirtual_casas.csv";
            Console.WriteLine("Carregando e processando dados do arquivo CSV...");
            var dataView = mlContext.Data.LoadFromTextFile<RealEstateData>(filePath, hasHeader: true, separatorChar: ',');

            // Pipeline para converter 'Preco' de string para float
            var dataProcessPipeline = mlContext.Transforms.Conversion.ConvertType(
                outputColumnName: "AreaFloat", // Criar uma nova coluna para armazenar o valor convertido
                inputColumnName: "Area",
                outputKind: DataKind.Single)
                .Append(mlContext.Transforms.Conversion.ConvertType(
                    outputColumnName: "PrecoFloat",
                    inputColumnName: "Preco",
                    outputKind: DataKind.Single))
                .Append(mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: "PrecoFloat"));

            var transformedData = dataProcessPipeline.Fit(dataView).Transform(dataView);

            DataDebuggingTools.PrintDataViewSchema(transformedData);  // Adicionado

            return transformedData;
        }



        public static ITransformer TrainModel(MLContext mlContext, IDataView trainingData)
        {
            Console.WriteLine("Treinando o modelo...");

            // Definir o pipeline de machine learning usando a coluna 'PrecoFloat'
            var pipeline = mlContext.Transforms.Text.FeaturizeText(outputColumnName: "DescriptionFeaturized", inputColumnName: "Nome")
                            .Append(mlContext.Transforms.Concatenate("Features", "DescriptionFeaturized", "Area", "PrecoFloat"))
                            .Append(mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: "PrecoFloat"))
                            .Append(mlContext.Regression.Trainers.FastTree());

            return pipeline.Fit(trainingData);
        }




        public static void PrepareData(RealEstateData input, ProcessedRealEstateData output)
        {
            output.Nome = input.Nome ?? string.Empty;
            output.Localizacao = input.Localizacao ?? string.Empty;

            var precoClean = input.Preco.Replace(" €", "").Replace(".", "").Replace(",", "").Trim();
            output.PrecoFloat = float.TryParse(precoClean, out float precoParsed) ? precoParsed : 0;

            var areaClean = input.Area.Replace(" m²", "").Replace(",", ".").Trim();
            output.AreaFloat = float.TryParse(areaClean, out float areaParsed) ? areaParsed : 0;
        }
    }

    public static class DataDebuggingTools
    {
        public static void PrintDataViewSchema(IDataView dataView)
        {
            var preview = dataView.Preview();
            Console.WriteLine("Visualização do esquema de dados:");
            foreach (var column in preview.Schema)
            {
                Console.WriteLine($"{column.Name}: {column.Type.RawType}");
            }
        }
    }

    public static class MachineLearningModel
    {
        public static event EventHandler<RealEstatePredictionEventArgs>? PredictionsReady;  // Adicionado
        public static void ExecuteMLNET(MLContext mlContext, ITransformer model, IDataView inputData)
        {
            Console.WriteLine("Executando modelo ML.NET...");
            var predictionFunction = mlContext.Model.CreatePredictionEngine<ProcessedRealEstateData, RealEstatePrediction>(model);

            var dataEnumerable = mlContext.Data.CreateEnumerable<ProcessedRealEstateData>(inputData, reuseRowObject: false);
            foreach (var item in dataEnumerable)
            {
                var prediction = predictionFunction.Predict(item);
                Console.WriteLine($"Predição de preço para {item.Nome}: {prediction.PredictedPrice}");
                OnPredictionsReady(new RealEstatePredictionEventArgs { PredictedPrice = prediction.PredictedPrice }); // Disparar evento aqui
            }
        }

        private static void OnPredictionsReady(RealEstatePredictionEventArgs e)  // Adicionado
        {
            PredictionsReady?.Invoke(null, e);
        }

        public static void SaveModel(MLContext mlContext, ITransformer model, DataViewSchema schema)
        {
            Console.WriteLine("A guardar o modelo treinado...");
            var modelPath = "model.zip";
            mlContext.Model.Save(model, schema, modelPath);
        }
    }

    public class RealEstatePredictionEventArgs : EventArgs // Adicionado
    {
        public float PredictedPrice { get; set; }
    }

    public class RealEstateData
    {
        [LoadColumn(1)]
        public string Nome { get; set; } = string.Empty;
        [LoadColumn(2)]
        public string Preco { get; set; } = string.Empty;
        [LoadColumn(3)]
        public string Area { get; set; } = string.Empty;
        [LoadColumn(4)]
        public string Localizacao { get; set; } = string.Empty;
    }

    public class ProcessedRealEstateData
    {
        public string Nome { get; set; } = string.Empty;
        public float PrecoFloat { get; set; } = 0.0f;
        public float AreaFloat { get; set; } = 0.0f;
        public string Localizacao { get; set; } = string.Empty;
    }

    public class RealEstatePrediction
    {
        [ColumnName("Score")]
        public float PredictedPrice;
    }

}
