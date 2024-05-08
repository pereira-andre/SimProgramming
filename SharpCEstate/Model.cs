/*
** ficheiro: Model.cs
**
** UC: 21179 - LDS @ UAb
**
** Alunos: 
** 2202880 - Andre Pereira
** 2203127 - Mario Prazeres
** 2204349 - Ruben Nunes
** 2203141 - Luciano Araujo
** 2201058 - Carla Campanico
*/

using System;
using Microsoft.ML;
using Microsoft.ML.Data;
using System.Threading.Tasks;

namespace SharpCEstate
{
    // Classe que representa os dados imobiliários
    public class RealEstateData
    {
        [LoadColumn(1)] public string Nome { get; set; } = string.Empty; // Nome do imóvel
        [LoadColumn(2)] public float Preco { get; set; } // Preço do imóvel
        [LoadColumn(3)] public float Area { get; set; } // Área do imóvel
        [LoadColumn(4)] public string Localizacao { get; set; } = string.Empty; // Localização do imóvel
    }

    // Classe que representa a previsão dos dados imobiliários
    public class RealEstatePrediction
    {
        [ColumnName("Score")]
        public float PredictedPrice { get; set; } // Preço previsto do imóvel
    }

    // Interface para previsão
    public interface IPredictor
    {
        Task<float> PredictPriceAsync(RealEstateData data); // Método para prever o preço do imóvel
    }

    // Processador de dados imobiliários
    public class RealEstateDataProcessor : IPredictor
    {
        private MLContext mlContext;
        private ITransformer? model; // Modelo é nulo por padrão
        private string modelPath;

        // Construtor
        public RealEstateDataProcessor(MLContext mlContext, string modelPath)
        {
            this.mlContext = mlContext;
            this.modelPath = modelPath;
            InitializeModel();
        }

        // Método privado para inicializar o modelo
        private void InitializeModel()
        {
            DataViewSchema modelSchema;
            this.model = this.mlContext.Model.Load(modelPath, out modelSchema);
        }

        // Método para prever o preço do imóvel de forma assíncrona
        public async Task<float> PredictPriceAsync(RealEstateData data)
        {
            if (model == null) throw new InvalidOperationException("Model not loaded.");
            return await Task.Run(() =>
            {
                var predEngine = mlContext.Model.CreatePredictionEngine<RealEstateData, RealEstatePrediction>(model);
                var prediction = predEngine.Predict(data);
                return prediction.PredictedPrice;
            });
        }

        // Método estático para carregar e preparar os dados
        public static IDataView LoadAndPrepareData(MLContext mlContext, string filePath)
        {
            return mlContext.Data.LoadFromTextFile<RealEstateData>(filePath, hasHeader: true, separatorChar: ',');
        }

        // Método estático para treinar o modelo
        public static ITransformer TrainModel(MLContext mlContext, IDataView trainingData)
        {
            var dataProcessPipeline = mlContext.Transforms.Conversion.MapValueToKey("LocalizacaoKey", "Localizacao")
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("LocalizacaoEncoded", "LocalizacaoKey"))
                .Append(mlContext.Transforms.Text.FeaturizeText("NomeFeaturized", "Nome"))
                .Append(mlContext.Transforms.Concatenate("Features", "Area", "LocalizacaoEncoded", "NomeFeaturized"))
                .Append(mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: "Preco"));
            var trainer = mlContext.Regression.Trainers.FastTree();
            return dataProcessPipeline.Append(trainer).Fit(trainingData);
        }

        // Método estático para salvar o modelo
        public static void SaveModel(MLContext mlContext, ITransformer model, string modelPath)
        {
            mlContext.Model.Save(model, null, modelPath);
        }
    }
}
