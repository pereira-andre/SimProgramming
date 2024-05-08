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
    public class ApplicationController
    {
        // Delegado para lidar com erros
        public delegate void ErrorHandler(string message);
        // Evento para notificar erros
        public event ErrorHandler ErrorOccurred;

        private static ApplicationController? instance;
        private MLContext mlContext = new MLContext(seed: 0);
        private ITransformer? model;
        private string modelPath = "./realEstateModel.zip";
        private string dataPath = "./data.csv";

        private ApplicationController()
        {
            // Assinatura de evento para lidar com erros
            ErrorOccurred += message => Console.WriteLine($"Erro: {message}");
        }

        // Instância Singleton
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

        // Inicializa a aplicação
        public Task StartApplicationAsync()
        {
            try
            {
                // Carrega e prepara os dados
                var dataView = RealEstateDataProcessor.LoadAndPrepareData(mlContext, dataPath);
                // Divide os dados em treino e teste
                var splitData = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);
                var trainingData = splitData.TrainSet;

                // Treina o modelo
                model = RealEstateDataProcessor.TrainModel(mlContext, trainingData);
                // Guarda o modelo treinado
                RealEstateDataProcessor.SaveModel(mlContext, model, modelPath);

                // Prepara a interface gráfica
                ViewUpdater.PrepareInterface();  
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro durante a inicialização da aplicação: {ex.Message}");
            }
            return Task.CompletedTask;
        }

        // Lida com pedidos do utilizador
        public async Task HandleUserRequestAsync(string[] inputs)
        {
            if (inputs.Length >= 3)
            {
                try
                {
                    // Extrai e valida os dados de entrada
                    float area = float.Parse(inputs[0].Trim());
                    string location = inputs[1].Trim();
                    string propertyType = inputs[2].Trim();

                    // Verifica se a área é válida
                    if (area <= 0)
                    {
                        ErrorOccurred?.Invoke("A área deve ser maior que zero.");
                        return;
                    }

                    // Verifica se o tipo de imóvel é válido
                    if (!System.Text.RegularExpressions.Regex.IsMatch(propertyType, "^t[0-9]$") && !System.Text.RegularExpressions.Regex.IsMatch(propertyType, "^t10$"))
                    {
                        ErrorOccurred?.Invoke("O tipo de imóvel deve ser entre t0 e t10.");
                        return;
                    }

                    // Processa e mostra a previsão de preço
                    var processor = new RealEstateDataProcessor(mlContext, modelPath); 
                    var predictedPrice = await processor.PredictPriceAsync(new RealEstateData { Area = area, Localizacao = location, Nome = propertyType });
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

        // Gera um relatório de preços por distrito
        public async Task GeneratePriceReportAsync(float area, string propertyType)
        {
            var predictions = new List<string> { "Distrito,Preço Previsto,Preço por m^2" };
            var reportsDir = Path.Combine(".", "reports"); 
            Directory.CreateDirectory(reportsDir); 
            var fileName = Path.Combine(reportsDir, $"precos_por_distrito_{area}m2_{propertyType}_{DateTime.Now:yyyyMMdd}.csv");

            var processor = new RealEstateDataProcessor(mlContext, modelPath); 
            foreach (var distrito in new string[] {
                "Aveiro", "Beja", "Braga", "Bragança", "Castelo Branco", "Coimbra",
                "Évora", "Faro", "Distrito da Guarda", "Leiria", "Lisboa", "Portalegre",
                "Porto", "Santarém", "Setúbal", "Viana do Castelo", "Vila Real", "Viseu"
            })
            {
                var data = new RealEstateData { Area = area, Localizacao = distrito, Nome = propertyType };
                var totalPrice = await processor.PredictPriceAsync(data);
                var pricePerSquareMeter = totalPrice / area;

                string formattedTotalPrice = totalPrice.ToString("F2", CultureInfo.InvariantCulture);
                string formattedPricePerSquareMeter = pricePerSquareMeter.ToString("F2", CultureInfo.InvariantCulture);

                predictions.Add($"{distrito},{formattedTotalPrice},{formattedPricePerSquareMeter}");
            }

            // Guarda o relatório em CSV
            File.WriteAllLines(fileName, predictions);
            Console.WriteLine($"Relatório de preços por distrito foi criado com sucesso: {fileName}");
        }
    }
}
