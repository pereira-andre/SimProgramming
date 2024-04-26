import pandas as pd

# Carrega os dados do arquivo CSV
df = pd.read_csv('imovirtual_casas.csv')

# Limpa e transforma as colunas de dados
df['Preço'] = df['Preço'].str.replace(' €', '').str.replace(' ', '')
df['Área'] = df['Área'].str.replace(' m²', '').str.replace(',', '.')
df['Localização'] = df['Localização'].str.split(', ').str[-1]
df['Nome'] = df['Nome'].str.extract('(T[0-9]+)')

# Remove linhas com valores vazios
df = df.dropna()

# Grava os resultados em um novo arquivo CSV
df.to_csv('dados_limpos.csv', index=False)

print("CSV limpo foi criado com sucesso!")
