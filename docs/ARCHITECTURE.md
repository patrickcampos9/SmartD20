# Arquitetura

## Objetivo

A arquitetura foi mantida pequena para facilitar o aprendizado, a depuração no celular e a futura investigação do protocolo BLE. Ela separa a interface da comunicação com o relógio sem introduzir estruturas que o projeto ainda não precisa.

Mesmo enxuta, a implementação deve seguir SOLID. O MVC organiza as responsabilidades principais; interfaces pequenas, injeção de dependências e componentes especializados permitem ampliar o aplicativo sem concentrar regras em Controllers ou Views.

O repositório possui três projetos:

- `SmartD20.Core`: Models, Controllers e Services independentes de plataforma.
- `D20Mobile`: aplicativo .NET MAUI, Views e composição de dependências.
- `D20Mobile.Tests`: testes unitários do núcleo.

## Componentes

### Models

Representam os dados manipulados pelo aplicativo.

- `WatchDevice`: identificação, nome, intensidade do sinal e origem real ou simulada.
- `HomeScreenModel`: dispositivos encontrados, seleção atual, conexão e mensagem de estado.

### Views

Contêm o XAML e o mínimo de código necessário para receber eventos da interface, chamar o controlador e redesenhar os controles.

`MainPage` não conhece detalhes de Bluetooth nem cria serviços diretamente.

### Controllers

Coordenam cada ação da tela. `HomeController` controla os estados de busca, seleção, conexão, desconexão e tratamento de falhas.

### Services

`IWatchConnectionService` define o contrato de comunicação sem depender de um modelo específico. No Android, `BluetoothWatchConnectionService` usa `IBluetoothLowEnergyTransport`, implementado pelo adaptador nativo `AndroidBluetoothLowEnergyTransport`. Nos demais destinos, `SimulatedWatchConnectionService` retorna dispositivos fictícios e simula o tempo das operações.

Essa composição é selecionada no registro de dependências em `MauiProgram`, sem alterar a View ou o Controller.

## Suporte a outros relógios

Recursos comuns, como busca BLE, conexão GATT e persistência, devem depender de contratos neutros em relação ao fabricante. Cada família de relógio deve encapsular descoberta de características, comandos e interpretação de pacotes em uma implementação própria.

Uma expansão futura pode introduzir contratos específicos, como um provedor de transporte e um codec de protocolo, desde que exista uma necessidade concreta. O controlador continuará coordenando casos de uso por abstrações e a injeção de dependências selecionará a implementação adequada. Esse desenho preserva o fluxo MVC e evita condicionais espalhadas por modelo de relógio.

Regras, validações e conversões compartilhadas devem existir em um único componente. Código parecido só deve ser extraído quando representa o mesmo comportamento, evitando tanto duplicidade quanto abstrações genéricas sem finalidade clara.

## Testabilidade

Todo método novo ou alterado deve possuir testes unitários. A lógica precisa permanecer fora de Views e APIs de plataforma para que Controllers, codecs, validadores e serviços possam ser testados com dependências simuladas. Manipuladores da interface devem apenas encaminhar ações para componentes testáveis.

## Ciclo de inicialização

Os serviços, o controlador, a View e o `AppShell` são registrados no contêiner de dependências. O `AppShell` é resolvido somente depois que `App.xaml` inicializa seus recursos globais, garantindo que cores e estilos estejam disponíveis durante a criação da tela.

## Evolução prevista

O transporte Android já é responsável por:

1. Solicitar e validar permissões do Android.
2. Buscar dispositivos próximos.
3. Abrir e encerrar a conexão GATT.

Os próximos incrementos serão responsáveis por:

1. Descobrir serviços e características.
2. Executar leituras e escritas de forma sequencial.
3. Encaminhar notificações recebidas para o controlador.

A montagem e interpretação dos pacotes do relógio deverá ficar em um componente separado do transporte BLE. Isso permitirá testar o protocolo apenas com sequências de bytes conhecidas.
