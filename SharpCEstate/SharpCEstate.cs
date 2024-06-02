/*
** ficheiro: SharpCEstate.cs
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
using System.Threading.Tasks;

namespace SharpCEstate
{
    class SharpCEstate
    {
        static async Task Main(string[] args)
        {
            // Cria uma instância do controlador da aplicação
            ApplicationController controller = ApplicationController.Instance;

            // Inicia a aplicação de forma assíncrona
            await controller.StartApplicationAsync();

            // Mostra o título do menu ao utilizador
            UserInteractionView.ShowMenuTitle();

            // Mostra o menu de opções ao utilizador
            UserInteractionView.ShowMenu();
        }
    }
}

// Define os códigos de erro que podem ocorrer na aplicação
public enum ErrorCode
{
    DataLoadingError,      // Erro ao carregar dados
    LoadModelError,        // Erro ao carregar o modelo
    ModelTrainingError,    // Erro ao treinar o modelo
    ModelSavingError,      // Erro ao salvar o modelo
    PredictionError,       // Erro ao fazer previsão
    ViewUpdateError,       // Erro ao atualizar a interface
    InvalidUserInputError, // Erro de entrada de utilizador inválida
    UnknownError           // Erro desconhecido
}
