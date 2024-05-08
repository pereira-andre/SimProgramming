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
    // Classe principal da aplicação
    class SharpCEstate
    {
        // Método principal assíncrono
        static async Task Main(string[] args)
        {
            // Exibe o título estilizado apenas uma vez
            UserInteractionView.ShowMenuTitle();

            // Inicia o controlador da aplicação
            ApplicationController controller = ApplicationController.Instance;
            await controller.StartApplicationAsync();

            // Exibe mensagem inicial
            Console.WriteLine("Bem-vindo ao SharpCEstate! Sua ferramenta de previsão de preços imobiliários.");
            UserInteractionView.Interact();
        }
    }
}
