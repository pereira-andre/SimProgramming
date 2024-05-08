from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
import time
import csv
import os
from collections import OrderedDict

# Classe para representar cada casa com suas informações.
class Casa:
    def __init__(self, indice, nome, preco, area, localizacao):
        self.indice = indice
        self.nome = nome
        self.preco = preco
        self.area = area
        self.localizacao = localizacao

# Configura o WebDriver do Selenium para usar o Chrome
driver = webdriver.Chrome()
wait = WebDriverWait(driver, 10)

# Função para obter o último índice do arquivo CSV
def get_last_index(file_name):
    try:
        with open(file_name, mode='r', newline='', encoding='utf-8') as file:
            last_line = list(csv.reader(file))[-1]
            return int(last_line[0])
    except (IOError, IndexError, ValueError):
        return 0

# Nome do arquivo CSV
file_name = 'imovirtual_casas.csv'

# Verifica se o arquivo CSV já existe para decidir se devemos escrever cabeçalhos
arquivo_existe = os.path.isfile(file_name)

# Obter o último índice de casa registrado no CSV
last_index = get_last_index(file_name)

# Prepara o arquivo CSV, escrevendo o cabeçalho se necessário
if not arquivo_existe:
    with open(file_name, mode='w', newline='', encoding='utf-8') as file:
        writer = csv.writer(file)
        writer.writerow(['Índice', 'Nome', 'Preço', 'Área', 'Localização'])

# Loop para iterar sobre as páginas desejadas
for i in range(1500, 2000):
    print(f"Iniciando a coleta da página {i}...")
    url = f'https://www.imovirtual.com/comprar/apartamento/?page={i}'
    driver.get(url)
    wait.until(EC.presence_of_all_elements_located((By.CSS_SELECTOR, 'article.offer-item')))
    anuncios = driver.find_elements(By.CSS_SELECTOR, 'article.offer-item')
    for anuncio in anuncios:
        nome = anuncio.find_element(By.CSS_SELECTOR, 'h3 > a > span').text
        preco = anuncio.find_element(By.CSS_SELECTOR, 'li.offer-item-price').text
        area = anuncio.find_element(By.CSS_SELECTOR, 'li.offer-item-area').text
        localizacao = anuncio.find_element(By.CSS_SELECTOR, 'p.text-nowrap').text

        last_index += 1
        casa = Casa(last_index, nome, preco, area, localizacao)

        # Escreve no arquivo CSV
        with open(file_name, mode='a', newline='', encoding='utf-8') as file:
            writer = csv.writer(file)
            writer.writerow([casa.indice, casa.nome, casa.preco, casa.area, casa.localizacao])

        # Coleta imediata, sem espera adicional
        time.sleep(0.1)

    # Aguarda um tempo antes de mudar para a próxima página
    time.sleep(3)

# Fechando o navegador
driver.quit()

# Remoção de duplicadas
casas_unicas = OrderedDict()
with open(file_name, mode='r', newline='', encoding='utf-8') as file:
    reader = csv.reader(file)
    header = next(reader)
    for row in reader:
        casas_unicas[(row[1], row[4])] = row  # Usando nome e localização como chave

# Reescreve o arquivo CSV sem duplicadas
with open(file_name, mode='w', newline='', encoding='utf-8') as file:
    writer = csv.writer(file)
    writer.writerow(header)
    for casa in casas_unicas.values():
        writer.writerow(casa)

print("A coleta de dados foi concluída e as duplicadas foram removidas.")
