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
    // Define métodos para prever preço, carregar, treinar e salvar um modelo
    public interface IPredictor
    {
        Task<float> PredictPriceAsync(RealEstateData data); // Método para prever o preço do imóvel de forma assíncrona
        void LoadModel(string modelPath); // Método para carregar um modelo a partir de um caminho especificado
        void TrainModel(IDataView trainingData); // Método para treinar um modelo usando dados fornecidos
        void SaveModel(string modelPath); // Método para salvar o modelo num caminho especificado
    }

    // Processador de dados imobiliários
    // Implementa a interface IPredictor
    public class RealEstateDataProcessor : IPredictor
    {
        private MLContext mlContext; // Contexto ML.NET
        private ITransformer? model; // Modelo de transformação ML.NET (nulo por padrão)
        private string modelPath; // Caminho para o modelo

        // Construtor que inicializa o contexto ML.NET e carrega o modelo
        public RealEstateDataProcessor(MLContext mlContext, string modelPath)
        {
            this.mlContext = mlContext;
            this.modelPath = modelPath;
            LoadModel(modelPath); // Carrega o modelo durante a inicialização
        }

        // Método para carregar o modelo a partir de um caminho especificado
        public void LoadModel(string modelPath)
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
                // Cria um motor de previsão a partir do modelo carregado
                var predEngine = mlContext.Model.CreatePredictionEngine<RealEstateData, RealEstatePrediction>(model);
                var prediction = predEngine.Predict(data); // Faz a previsão com base nos dados fornecidos
                return prediction.PredictedPrice; // Retorna o preço previsto
            });
        }

        // Método para treinar o modelo usando dados fornecidos
        public void TrainModel(IDataView trainingData)
        {
            var dataProcessPipeline = mlContext.Transforms.Conversion.MapValueToKey("LocalizacaoKey", "Localizacao")
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("LocalizacaoEncoded", "LocalizacaoKey"))
                .Append(mlContext.Transforms.Text.FeaturizeText("NomeFeaturized", "Nome"))
                .Append(mlContext.Transforms.Concatenate("Features", "Area", "LocalizacaoEncoded", "NomeFeaturized"))
                .Append(mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: "Preco"));

            // Treinador de regressão
            var trainer = mlContext.Regression.Trainers.FastTree();
            // Treina o modelo com o pipeline de processamento de dados e o treinador de regressão
            this.model = dataProcessPipeline.Append(trainer).Fit(trainingData);
        }

        // Método para salvar o modelo num caminho especificado
        public void SaveModel(string modelPath)
        {
            if (model == null) throw new InvalidOperationException("Model not trained.");
            mlContext.Model.Save(model, null, modelPath);
        }

        // Método estático para carregar e preparar os dados a partir de um ficheiro
        public static IDataView LoadAndPrepareData(MLContext mlContext, string filePath)
        {
            return mlContext.Data.LoadFromTextFile<RealEstateData>(filePath, hasHeader: true, separatorChar: ',');
        }
    }
}
