# Auditoria de qualidade

Última revisão: 21 de setembro de 2026.

## Resultado

O código de aplicação atende às regras atuais de arquitetura, SOLID, ausência de duplicidade e testes unitários dentro do escopo implementado.

| Regra | Situação | Evidência |
| --- | --- | --- |
| MVC simples | Atende | View no projeto MAUI; Models e Controllers no núcleo independente de plataforma. |
| Responsabilidades separadas | Atende | A View encaminha eventos, o Controller coordena o fluxo e os Services separam aplicação, transporte BLE e simulação. |
| Inversão de dependência | Atende | `HomeController` depende de `IWatchConnectionService`. |
| Extensível para outros relógios | Atende | Contratos e modelos genéricos usam `Watch`; detalhes D20 não fazem parte da abstração de transporte. |
| Ausência de duplicidade | Atende | A coleção do Model é a única fonte dos dispositivos e o tratamento assíncrono comum foi centralizado no Controller. |
| Operações concorrentes | Atende | O Controller ignora novos comandos enquanto existe uma operação em andamento. |
| Cancelamento e falhas | Atende | Busca, conexão e desconexão tratam cancelamento e exceções, restaurando o estado de atividade. |
| Testes unitários | Atende | 49 testes aprovados, com 100% de linhas e ramificações no `SmartD20.Core`. |
| Automação | Atende | O workflow `tests.yml` executa a suíte em pushes e pull requests para `main`. |
| Avisos de compilação | Atende | `Directory.Build.props` promove avisos a erros em todos os projetos. |

## Correções realizadas

- Extração de `SmartD20.Core`, removendo a dependência do núcleo em .NET MAUI.
- Criação de `D20Mobile.Tests` com cobertura de Models, Controller e serviço simulado.
- Substituição de `D20Device` por `WatchDevice`.
- Substituição de `ID20ConnectionService` por `IWatchConnectionService`.
- Substituição de `SimulatedD20ConnectionService` por `SimulatedWatchConnectionService`.
- Remoção da coleção duplicada mantida pela View.
- Centralização do tratamento repetido de atividade, cancelamento e falhas.
- Proteção contra buscas, conexões ou desconexões concorrentes.
- Inclusão dos projetos de núcleo e testes na solução.
- Implementação da busca BLE e conexão GATT nativas no Android.
- Extração da coleta de resultados e do estado da conexão para componentes C# testáveis.

## Validação executada

```text
Testes: 49 aprovados, 0 falhas, 0 ignorados
Cobertura do SmartD20.Core: 100% de linhas e 100% de ramificações
Android: compilação concluída com 0 avisos e 0 erros
Windows: compilação concluída com 0 avisos e 0 erros
```
