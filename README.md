# Rinha de Backend 2025: O meu projeto

Uma implementação otimizada e performática para a [Rinha de Backend 2025](https://github.com/zanfranceschi/rinha-de-backend-2025), um desafio de alta concorrência focado em web services e bases de dados. Este projeto visa alcançar o melhor desempenho possível em cenários de alta carga.

---

## 🚀 Tecnologias Utilizadas

Este projeto foi construído com foco em performance e escalabilidade, utilizando as seguintes tecnologias:

* **C# e ASP.NET Core**: A base da nossa API, escolhida pela sua performance, maturidade e suporte robusto para programação assíncrona.
* **Redis**: Utilizado como banco de dados principal, aproveitando a sua velocidade e estrutura de dados eficiente para lidar com grandes volumes de requisições.
* **Nginx**: Atua como load balancer, distribuindo o tráfego entre as instâncias da API para garantir alta disponibilidade e otimizar o uso de recursos.
* **Docker e Docker Compose**: Ferramentas essenciais para orquestração e gerenciamento dos containers, garantindo que o ambiente de desenvolvimento e produção seja consistente e fácil de configurar.
* **Programação Assíncrona**: Todas as operações de I/O são executadas de forma assíncrona, maximizando o uso da CPU e minimizando o tempo de espera das requisições.

---

## 🛠️ Como Rodar o Projeto

Para colocar o projeto em funcionamento, você precisará ter o [Docker](https://www.docker.com/) e o [Docker Compose](https://docs.docker.com/compose/) instalados na sua máquina.

1.  **Inicie os serviços principais**:
    
    Abra o terminal na raiz do projeto e execute o comando:
    
    ```bash
    docker-compose up -d
    ```
    
2.  **Inicie os processadores de pagamento**:
    
    Navegue até a pasta `payment-processors` e execute o Docker Compose para os serviços adicionais:
    
    ```bash
    cd payment-processors
    docker-compose up -d
    ```

Após a execução desses comandos, todos os serviços estarão rodando e acessíveis.

---

## 📁 Estrutura do Projeto

_Em breve..._

---

## 📈 Considerações de Performance

_Em breve..._

---

## 🏆 Resultados e Links

Acompanhe a classificação e os resultados da Rinha de Backend 2025 no repositório oficial:

[https://github.com/zanfranceschi/rinha-de-backend-2025](https://github.com/zanfranceschi/rinha-de-backend-2025)

---

> Este projeto foi desenvolvido para o desafio Rinha de Backend 2025.
