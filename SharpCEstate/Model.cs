//Model.cs
using System;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Text;

namespace SharpCEstate
{
    // Definição das classes de dados
    public class RealEstateData
    {
        [LoadColumn(1)] public string Nome { get; set; } = String.Empty;
        [LoadColumn(2)] public float Preco { get; set; }
        [LoadColumn(3)] public float Area { get; set; }
        [LoadColumn(4)] public string Localizacao { get; set; } = String.Empty;
    }

    public class RealEstatePrediction
    {
        [ColumnName("Score")]
        public float PredictedPrice { get; set; }
    }

    // Classe para processamento de dados e operações do modelo
    public static class RealEstateDataProcessor
    {
        // Carrega e prepara os dados de um arquivo CSV
        public static IDataView LoadAndPrepareData(MLContext mlContext, string filePath)
        {
            return mlContext.Data.LoadFromTextFile<RealEstateData>(
                filePath, hasHeader: true, separatorChar: ',');
        }

        // Configura e treina o modelo de Machine Learning
        public static ITransformer TrainModel(MLContext mlContext, IDataView trainingData)
        {
            var dataProcessPipeline = mlContext.Transforms.Conversion.MapValueToKey("LocalizacaoKey", "Localizacao")
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("LocalizacaoEncoded", "LocalizacaoKey"))
                .Append(mlContext.Transforms.Text.FeaturizeText("NomeFeaturized", "Nome"))
                .Append(mlContext.Transforms.Concatenate("Features", "Area", "LocalizacaoEncoded", "NomeFeaturized"))
                .Append(mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: "Preco"));

            var trainer = mlContext.Regression.Trainers.FastTree();
            var trainingPipeline = dataProcessPipeline.Append(trainer);

            return trainingPipeline.Fit(trainingData);
        }

        // Salva o modelo treinado em um arquivo
        public static void SaveModel(MLContext mlContext, ITransformer model, string modelPath)
        {
            mlContext.Model.Save(model, null, modelPath);
        }

        // Define um delegate para o evento de predição concluída
        public delegate void PredictionCompletedHandler(RealEstatePrediction prediction);
        public static event PredictionCompletedHandler? OnPredictionCompleted;

        // Carrega o modelo de um arquivo e faz uma predição
        public static RealEstatePrediction PredictPrice(MLContext mlContext, string modelPath, RealEstateData inputData)
        {
            DataViewSchema modelSchema;
            ITransformer model = mlContext.Model.Load(modelPath, out modelSchema);
            var predEngine = mlContext.Model.CreatePredictionEngine<RealEstateData, RealEstatePrediction>(model);
            var prediction = predEngine.Predict(inputData);

            // Disparar o evento de predição concluída
            OnPredictionCompleted?.Invoke(prediction);

            return prediction;
        }
    }
}
