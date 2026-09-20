# Arquitetura

## Objetivo

A arquitetura foi mantida pequena para facilitar o aprendizado, a depuração no celular e a futura investigação do protocolo BLE. Ela separa a interface da comunicação com o relógio sem introduzir estruturas que o projeto ainda não precisa.

## Componentes

### Models

Representam os dados manipulados pelo aplicativo.

- `D20Device`: identificação, nome, intensidade do sinal e origem real ou simulada.
- `HomeScreenModel`: dispositivos encontrados, seleção atual, conexão e mensagem de estado.

### Views

Contêm o XAML e o mínimo de código necessário para receber eventos da interface, chamar o controlador e redesenhar os controles.

`MainPage` não conhece detalhes de Bluetooth nem cria serviços diretamente.

### Controllers

Coordenam cada ação da tela. `HomeController` controla os estados de busca, seleção, conexão, desconexão e tratamento de falhas.

### Services

`ID20ConnectionService` define o contrato de comunicação. A implementação atual, `SimulatedD20ConnectionService`, retorna dispositivos fictícios e simula o tempo das operações.

A implementação BLE real deverá cumprir o mesmo contrato. Dessa forma, a troca será feita no registro de dependências em `MauiProgram`, sem alterar a View.

## Ciclo de inicialização

Os serviços, o controlador, a View e o `AppShell` são registrados no contêiner de dependências. O `AppShell` é resolvido somente depois que `App.xaml` inicializa seus recursos globais, garantindo que cores e estilos estejam disponíveis durante a criação da tela.

## Evolução prevista

Quando a comunicação real for adicionada, o serviço BLE será responsável por:

1. Solicitar e validar permissões do Android.
2. Buscar dispositivos próximos.
3. Abrir e encerrar a conexão GATT.
4. Descobrir serviços e características.
5. Executar leituras e escritas de forma sequencial.
6. Encaminhar notificações recebidas para o controlador.

A montagem e interpretação dos pacotes do relógio deverá ficar em um componente separado do transporte BLE. Isso permitirá testar o protocolo apenas com sequências de bytes conhecidas.

