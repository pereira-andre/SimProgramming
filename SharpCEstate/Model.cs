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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace SharpCEstate
{
    // Classe que representa os dados imobiliários
    public class RealEstateData
    {
        [LoadColumn(1)] public string Nome { get; set; } = string.Empty;  // Nome do imóvel
        [LoadColumn(2)] public float Preco { get; set; }                 // Preço do imóvel
        [LoadColumn(3)] public float Area { get; set; }                  // Área do imóvel
        [LoadColumn(4)] public string Localizacao { get; set; } = string.Empty; // Localização do imóvel
    }

    // Classe que representa a previsão imobiliária
    public class RealEstatePrediction
    {
        [ColumnName("Score")]
        public float PredictedPrice { get; set; } // Preço previsto
    }

    // Interface que define as operações de previsão
    public interface IPredictor
    {
        Task<float> PredictPriceAsync(RealEstateData data);              // Prever preço de um imóvel
        void LoadModel(string modelPath);                                // Carregar modelo de previsão
        void TrainModel(IDataView trainingData);                         // Treinar modelo de previsão
        void SaveModel(string modelPath);                                // Salvar modelo treinado
        Task<string> GeneratePriceReportAsync(float area, string propertyType); // Gerar relatório de preços
        Task<string> GenerateReportForMaxBudgetAsync(float maxBudget, string propertyType); // Gerar relatório para orçamento máximo
    }

    // Classe que processa os dados imobiliários e implementa a interface IPredictor
    public class RealEstateDataProcessor : IPredictor
    {
        private MLContext mlContext;
        private ITransformer? model;
        private string modelPath;

        // Delegates
        public delegate void PredictPriceAsyncHandler(float predictedPrice);
        public delegate void ModelAndDataLoadedHandler();
        public delegate void ShowReportHandler(string reportPath);
        
        // Eventos
        public event PredictPriceAsyncHandler? PredictPriceAsyncCompleted;
        public event ModelAndDataLoadedHandler? ModelAndDataLoaded;

        // Construtor que inicializa o contexto de ML e carrega o modelo
        public RealEstateDataProcessor(MLContext mlContext, string modelPath)
        {
            this.mlContext = mlContext;
            this.modelPath = modelPath;
            LoadModel(modelPath);
        }

        // Método para carregar o modelo de previsão
        public void LoadModel(string modelPath)
        {
            try
            {
                DataViewSchema modelSchema;
                this.model = this.mlContext.Model.Load(modelPath, out modelSchema);
                OnModelAndDataLoaded(); // Evento ModelAndDataLoaded
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao carregar modelo: Arquivo não encontrado", ex);
            }
        }

        // Método assíncrono para prever o preço de um imóvel
        public async Task<float> PredictPriceAsync(RealEstateData data)
        {
            if (model == null) throw new InvalidOperationException("Model not loaded."); 
            return await Task.Run(() =>
            {
                var predEngine = mlContext.Model.CreatePredictionEngine<RealEstateData, RealEstatePrediction>(model);
                var prediction = predEngine.Predict(data);
                OnPredictPriceAsyncCompleted(prediction.PredictedPrice); // Evento PredictPriceAsyncCompleted
                return prediction.PredictedPrice;
            });
        }

        // Método para treinar o modelo de previsão
        public void TrainModel(IDataView trainingData)
        {
            var dataProcessPipeline = mlContext.Transforms.Conversion.MapValueToKey("LocalizacaoKey", "Localizacao")
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("LocalizacaoEncoded", "LocalizacaoKey"))
                .Append(mlContext.Transforms.Text.FeaturizeText("NomeFeaturized", "Nome"))
                .Append(mlContext.Transforms.Concatenate("Features", "Area", "LocalizacaoEncoded", "NomeFeaturized"))
                .Append(mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: "Preco"));

            var trainer = mlContext.Regression.Trainers.FastTree();
            this.model = dataProcessPipeline.Append(trainer).Fit(trainingData);
        }

        // Método para salvar o modelo treinado
        public void SaveModel(string modelPath)
        {
            if (model == null) throw new InvalidOperationException("Model not trained.");
            mlContext.Model.Save(model, null, modelPath);
        }

        // Método para carregar e preparar os dados a partir de um ficheiro CSV
        public static IDataView LoadAndPrepareData(MLContext mlContext, string filePath)
        {
            return mlContext.Data.LoadFromTextFile<RealEstateData>(filePath, hasHeader: true, separatorChar: ',');
        }

        // Método assíncrono para gerar um relatório de preços
        public async Task<string> GeneratePriceReportAsync(float area, string propertyType)
        {
            var predictions = new List<(string District, float TotalPrice, float PricePerSquareMeter)>();
            foreach (var distrito in new string[] {
                "Aveiro", "Beja", "Braga", "Bragança", "Castelo Branco", "Coimbra",
                "Évora", "Faro", "Guarda", "Leiria", "Lisboa", "Portalegre",
                "Porto", "Santarém", "Setúbal", "Viana do Castelo", "Vila Real", "Viseu"
            })
            {
                var data = new RealEstateData { Area = area, Localizacao = distrito, Nome = propertyType };
                var totalPrice = await PredictPriceAsync(data);
                var pricePerSquareMeter = totalPrice / area;
                predictions.Add((District: distrito, TotalPrice: totalPrice, PricePerSquareMeter: pricePerSquareMeter));
            }

            // Ordenar previsões por ordem decrescente de preço total
            predictions = predictions.OrderByDescending(p => p.TotalPrice).ToList();

            return GenerateHTMLReport(predictions, propertyType, area);
        }

        // Método para gerar o relatório HTML de preços
        private string GenerateHTMLReport(List<(string District, float TotalPrice, float PricePerSquareMeter)> predictions, string propertyType, float area)
        {
            string fileName = $"./reports/price_report_{propertyType}_{area}_{DateTime.Now:yyyyMMdd}.html";
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                sw.WriteLine("<html><head>");
                sw.WriteLine("<script src='https://cdn.jsdelivr.net/npm/chart.js'></script>");
                sw.WriteLine("<style>");
                sw.WriteLine(".logo { float: right; width: 150px; height: 150px; }");
                sw.WriteLine("</style>");
                sw.WriteLine("</head><body>");
                sw.WriteLine("<img class='logo' src='https://github.com/pereira-andre/SimProgramming/raw/main/SharpCEstate/logo.png' alt='SharpCEstate Logo'>");
                sw.WriteLine("<h1>SharpCEstate Relatório Imobiliário</h1>");
                sw.WriteLine("<h2>Preços dos imóveis em Portugal.</h2>");
                sw.WriteLine($"<h3>Gerado em: {DateTime.Now}, com área: {area} m² e tipo de imóvel: {propertyType}</h3>");

                // Adicionar cálculos estatísticos
                float avgPrice = predictions.Average(p => p.TotalPrice);
                float maxPrice = predictions.Max(p => p.TotalPrice);
                float minPrice = predictions.Min(p => p.TotalPrice);
                sw.WriteLine($"<p>Preço Médio: {avgPrice:F2} €</p>");
                sw.WriteLine($"<p>Preço Máximo: {maxPrice:F2} €</p>");
                sw.WriteLine($"<p>Preço Mínimo: {minPrice:F2} €</p>");

                sw.WriteLine("<table border='1'><tr><th>Distrito</th><th>Preço Total</th><th>Preço por m²</th></tr>");
                foreach (var prediction in predictions)
                {
                    sw.WriteLine($"<tr><td>{prediction.District}</td><td>{prediction.TotalPrice:F2}</td><td>{prediction.PricePerSquareMeter:F2}</td></tr>");
                }
                sw.WriteLine("</table>");
                sw.WriteLine("<canvas id='totalPriceChart'></canvas>");
                sw.WriteLine("<canvas id='pricePerSqMChart'></canvas>");
                sw.WriteLine("<script>");
                sw.WriteLine("var ctx = document.getElementById('totalPriceChart').getContext('2d');");
                sw.WriteLine("var totalPriceChart = new Chart(ctx, { type: 'bar', data: { labels: [");
                sw.WriteLine(String.Join(", ", predictions.Select(p => $"'{p.District}'")));
                sw.WriteLine("], datasets: [{ label: 'Preço Total', data: [");
                sw.WriteLine(String.Join(", ", predictions.Select(p => p.TotalPrice)));
                sw.WriteLine("] }] } });");
                sw.WriteLine("var ctx2 = document.getElementById('pricePerSqMChart').getContext('2d');");
                sw.WriteLine("var pricePerSqMChart = new Chart(ctx2, { type: 'bar', data: { labels: [");
                sw.WriteLine(String.Join(", ", predictions.Select(p => $"'{p.District}'")));
                sw.WriteLine("], datasets: [{ label: 'Preço por m²', data: [");
                sw.WriteLine(String.Join(", ", predictions.Select(p => p.PricePerSquareMeter)));
                sw.WriteLine("] }] } });");
                sw.WriteLine("</script>");
                sw.WriteLine("</body></html>");
            }
            return fileName;
        }

        // Método assíncrono para gerar um relatório com base no orçamento máximo
        public async Task<string> GenerateReportForMaxBudgetAsync(float maxBudget, string propertyType)
        {
            var predictions = new List<(string District, float MaxArea, int MaxRooms, float AvgPricePerSqM)>();
            foreach (var distrito in new string[] {
                "Aveiro", "Beja", "Braga", "Bragança", "Castelo Branco", "Coimbra",
                "Évora", "Faro", "Guarda", "Leiria", "Lisboa", "Portalegre",
                "Porto", "Santarém", "Setúbal", "Viana do Castelo", "Vila Real", "Viseu"
            })
            {
                var data = new RealEstateData { Area = 100, Localizacao = distrito, Nome = propertyType };
                var predictedPricePerSqM = await PredictPriceAsync(data) / 100;
                var maxArea = maxBudget / predictedPricePerSqM;
                var maxRooms = (int)(maxArea / 50);
                var avgPricePerSqM = predictedPricePerSqM;
                predictions.Add((District: distrito, MaxArea: maxArea, MaxRooms: maxRooms, AvgPricePerSqM: avgPricePerSqM));
            }

            // Ordenar previsões por ordem decrescente de área máxima
            predictions = predictions.OrderByDescending(p => p.MaxArea).ToList();

            return GenerateHTMLReportForMaxBudget(predictions, propertyType, maxBudget);
        }

        // Método para gerar o relatório HTML com base no orçamento máximo
        private string GenerateHTMLReportForMaxBudget(List<(string District, float MaxArea, int MaxRooms, float AvgPricePerSqM)> predictions, string propertyType, float maxBudget)
        {
            string fileName = $"./reports/budget_report_{propertyType}_{maxBudget}_{DateTime.Now:yyyyMMdd}.html";
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                sw.WriteLine("<html><head>");
                sw.WriteLine("<script src='https://cdn.jsdelivr.net/npm/chart.js'></script>");
                sw.WriteLine("<style>");
                sw.WriteLine(".logo { float: right; width: 150px; height: 150px; }");
                sw.WriteLine("</style>");
                sw.WriteLine("</head><body>");
                sw.WriteLine("<img class='logo' src='https://github.com/pereira-andre/SimProgramming/raw/main/SharpCEstate/logo.png' alt='SharpCEstate Logo'>");
                sw.WriteLine("<h1>SharpCEstate Relatório Imobiliário</h1>");
                sw.WriteLine("<h2>Áreas Máximas de Imóveis em Portugal com base no orçamento.</h2>");
                sw.WriteLine($"<h3>Gerado em: {DateTime.Now}, com orçamento máximo: {maxBudget} € e tipo de imóvel: {propertyType}</h3>");

                // Adicionar cálculos estatísticos
                float avgMaxArea = predictions.Average(p => p.MaxArea);
                float maxMaxArea = predictions.Max(p => p.MaxArea);
                float minMaxArea = predictions.Min(p => p.MaxArea);
                sw.WriteLine($"<p>Área Máxima Média: {avgMaxArea:F2} m²</p>");
                sw.WriteLine($"<p>Área Máxima Máxima: {maxMaxArea:F2} m²</p>");
                sw.WriteLine($"<p>Área Máxima Mínima: {minMaxArea:F2} m²</p>");

                sw.WriteLine("<table border='1'><tr><th>Distrito</th><th>Área Máxima (m²)</th><th>Máximo de Assoalhadas</th><th>Preço Médio por m² (€)</th></tr>");
                foreach (var prediction in predictions)
                {
                    sw.WriteLine($"<tr><td>{prediction.District}</td><td>{prediction.MaxArea:F2}</td><td>{prediction.MaxRooms}</td><td>{prediction.AvgPricePerSqM:F2}</td></tr>");
                }
                sw.WriteLine("</table>");
                sw.WriteLine("<canvas id='maxAreaChart'></canvas>");
                sw.WriteLine("<canvas id='avgPricePerSqMChart'></canvas>");
                sw.WriteLine("<script>");
                sw.WriteLine("var ctx = document.getElementById('maxAreaChart').getContext('2d');");
                sw.WriteLine("var maxAreaChart = new Chart(ctx, { type: 'bar', data: { labels: [");
                sw.WriteLine(String.Join(", ", predictions.Select(p => $"'{p.District}'")));
                sw.WriteLine("], datasets: [{ label: 'Área Máxima (m²)', data: [");
                sw.WriteLine(String.Join(", ", predictions.Select(p => p.MaxArea)));
                sw.WriteLine("] }] } });");
                sw.WriteLine("var ctx2 = document.getElementById('avgPricePerSqMChart').getContext('2d');");
                sw.WriteLine("var avgPricePerSqMChart = new Chart(ctx2, { type: 'bar', data: { labels: [");
                sw.WriteLine(String.Join(", ", predictions.Select(p => $"'{p.District}'")));
                sw.WriteLine("], datasets: [{ label: 'Preço Médio por m² (€)', data: [");
                sw.WriteLine(String.Join(", ", predictions.Select(p => p.AvgPricePerSqM)));
                sw.WriteLine("] }] } });");
                sw.WriteLine("</script>");
                sw.WriteLine("</body></html>");
            }
            return fileName;
        }

        // Evento ModelAndDataLoaded
        protected virtual void OnModelAndDataLoaded()
        {
            ModelAndDataLoaded?.Invoke();
        }

        // Evento PredictPriceAsyncCompleted
        protected virtual void OnPredictPriceAsyncCompleted(float predictedPrice)
        {
            PredictPriceAsyncCompleted?.Invoke(predictedPrice);
        }
    }
}
