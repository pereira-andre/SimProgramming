/*
** ficheiro: Controller.cs
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
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.ML;

namespace SharpCEstate
{
    // Controlador da aplicação
    public class ApplicationController
    {
        // Delegado para manipulação de erros
        public delegate void ErrorHandler(string message);
        // Evento para notificar erros
        public event ErrorHandler ErrorOccurred;

        // Instância única do controlador (singleton)
        private static ApplicationController? instance;
        private MLContext mlContext = new MLContext(seed: 0); // Contexto ML.NET
        private IPredictor predictor; // Instância do preditor

        private string modelPath = "./realEstateModel.zip"; // Caminho para o modelo
        private string dataPath = "./data.csv"; // Caminho para os dados

        // Construtor privado (singleton)
        private ApplicationController()
        {
            // Regista o manipulador de erros padrão
            ErrorOccurred += message => Console.WriteLine($"Erro: {message}");
            // Inicializa o preditor
            predictor = new RealEstateDataProcessor(mlContext, modelPath);
        }

        // Propriedade para obter a instância única do controlador
        public static ApplicationController Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ApplicationController();
                }
                return instance;
            }
        }

        // Método para iniciar a aplicação de forma assíncrona
        public async Task StartApplicationAsync()
        {
            try
            {
                // Carrega e prepara os dados
                var dataView = RealEstateDataProcessor.LoadAndPrepareData(mlContext, dataPath);
                var splitData = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2); // Divide os dados em treino e teste
                var trainingData = splitData.TrainSet;

                // Treina e salva o modelo
                predictor.TrainModel(trainingData);
                predictor.SaveModel(modelPath);

                // Prepara a interface do utilizador
                ViewUpdater.PrepareInterface();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro durante a inicialização da aplicação: {ex.Message}");
            }
            await Task.CompletedTask;
        }

        // Método para lidar com as solicitações do utilizador
        public async Task HandleUserRequestAsync(string[] inputs)
        {
            if (inputs.Length >= 3)
            {
                try
                {
                    // Processa as entradas do utilizador
                    float area = float.Parse(inputs[0].Trim());
                    string location = inputs[1].Trim();
                    string propertyType = inputs[2].Trim();

                    // Valida as entradas
                    if (area <= 0)
                    {
                        ErrorOccurred?.Invoke("A área deve ser maior que zero.");
                        return;
                    }

                    if (!System.Text.RegularExpressions.Regex.IsMatch(propertyType, "^t[0-9]$") && !System.Text.RegularExpressions.Regex.IsMatch(propertyType, "^t10$"))
                    {
                        ErrorOccurred?.Invoke("O tipo de imóvel deve ser entre t0 e t10.");
                        return;
                    }

                    // Faz a previsão de preço e atualiza a interface
                    var predictedPrice = await predictor.PredictPriceAsync(new RealEstateData { Area = area, Localizacao = location, Nome = propertyType });
                    ViewUpdater.ShowForecast(predictedPrice);
                }
                catch (FormatException)
                {
                    ErrorOccurred?.Invoke("Formato de entrada inválido para número.");
                }
            }
            else
            {
                ErrorOccurred?.Invoke("Número insuficiente de entradas. Por favor, forneça área, localização e tipo.");
            }
        }

        // Método para gerar um relatório de preços por distrito
        public async Task GeneratePriceReportAsync(float area, string propertyType)
        {
            var predictions = new List<(string District, float TotalPrice, float PricePerSquareMeter)>();

            // Itera por todos os distritos e faz previsões
            foreach (var distrito in new string[] {
                "Aveiro", "Beja", "Braga", "Bragança", "Castelo Branco", "Coimbra",
                "Évora", "Faro", "Guarda", "Leiria", "Lisboa", "Portalegre",
                "Porto", "Santarém", "Setúbal", "Viana do Castelo", "Vila Real", "Viseu"
            })
            {
                var data = new RealEstateData { Area = area, Localizacao = distrito, Nome = propertyType };
                var totalPrice = await predictor.PredictPriceAsync(data);
                var pricePerSquareMeter = totalPrice / area;

                predictions.Add((District: distrito, TotalPrice: totalPrice, PricePerSquareMeter: pricePerSquareMeter));
            }

            // Gera o relatório HTML
            GenerateHTMLReport(predictions, propertyType, area);
        }

        // Método privado para gerar um relatório HTML
        private void GenerateHTMLReport(List<(string District, float TotalPrice, float PricePerSquareMeter)> predictions, string propertyType, float area)
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

                // Adiciona o logotipo da empresa ao relatório
                sw.WriteLine("<img class='logo' src='https://github.com/pereira-andre/SimProgramming/raw/main/SharpCEstate/logo.png' alt='SharpCEstate Logo'>");

                // Adiciona o título e informações do relatório
                sw.WriteLine("<h1>SharpCEstate Relatório Imobiliário</h1>");
                sw.WriteLine("<h2>Preços dos imóveis em Portugal.</h2>");
                sw.WriteLine($"<h3>Gerado em: {DateTime.Now}, com área: {area} m² e tipo de imóvel: {propertyType}</h3>");

                // Cria a tabela com os resultados das previsões
                sw.WriteLine("<table border='1'><tr><th>Distrito</th><th>Preço Total</th><th>Preço por m²</th></tr>");
                foreach (var prediction in predictions)
                {
                    sw.WriteLine($"<tr><td>{prediction.District}</td><td>{prediction.TotalPrice:F2}</td><td>{prediction.PricePerSquareMeter:F2}</td></tr>");
                }
                sw.WriteLine("</table>");

                // Adiciona gráficos com os dados das previsões
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
        }
    }
}
