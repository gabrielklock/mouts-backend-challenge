# 🚀 Como rodar o projeto

Existem duas formas de executar a aplicação:

---

## 🐳 Usando Docker (recomendado)

1. Acesse a pasta do backend:

```
cd mouts-backend-challenge/template/backend
```

2. Execute o comando:

```
docker-compose up
```

👉 Isso irá subir:

- API  
- Banco de dados  

---

## 💻 Usando Visual Studio

1. Abra a solution no Visual Studio  
2. Altere o **Startup Project/Profile** para `docker-compose`  
3. Execute normalmente (`F5` ou `Ctrl + F5`)  

---

# 🔐 Autenticação

A API utiliza **autenticação com controle de acesso por Roles**.

Já existe um usuário pré-cadastrado para facilitar os testes:

- **Email:** admin@developerstore.com  
- **Senha:** Admin@123  

👉 Utilize esse usuário para autenticar e acessar os endpoints protegidos.

---

# 📄 Documentação da API

Para visualizar e testar os endpoints da API, utilize o Swagger.

Após subir o projeto com Docker, acesse:

👉 http://localhost:8080/swagger/index.html

No Swagger é possível:

- Visualizar todos os endpoints disponíveis  
- Testar requisições diretamente  
- Ver os modelos de entrada e saída  

---

# 📌 Observações

- Os endpoints possuem autorização baseada em **Roles**  
- Certifique-se de estar autenticado antes de consumir endpoints protegidos  
- O Swagger facilita o fluxo de testes e exploração da API  
