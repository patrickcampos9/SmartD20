# Guia de desenvolvimento

Este guia complementa as regras obrigatórias de `AGENTS.md` e descreve o fluxo recomendado para trabalhar no SmartD20.

## Preparação do ambiente

Instale o Visual Studio com a carga de trabalho .NET MAUI, o SDK do .NET 10 e o SDK Android API 36. Para testes em aparelho físico, habilite a depuração USB e autorize o computador no Android.

Abra `D20Mobile.slnx`, selecione o projeto `D20Mobile` e escolha o dispositivo de destino.

Execute a suíte unitária com:

```powershell
dotnet test D20Mobile.Tests/D20Mobile.Tests.csproj
```

Avisos de compilação são tratados como erros em todos os projetos. O GitHub executa a mesma suíte automaticamente em pushes e pull requests direcionados à branch `main`.

## Fluxo de desenvolvimento

1. Confirme que a branch local está atualizada e sem alterações inesperadas.
2. Faça a mudança na camada responsável pelo comportamento.
3. Crie ou atualize os testes unitários de cada método criado ou alterado.
4. Procure lógica equivalente e elimine duplicações.
5. Use o serviço simulado para validar fluxos independentes do relógio.
6. Execute todos os testes unitários.
7. Compile os destinos afetados.
8. Execute no Android quando houver mudança em inicialização, XAML ou integração de plataforma.
9. Atualize a documentação relacionada.
10. Revise os arquivos antes do commit para evitar artefatos e dados locais.

Uma mudança que não cumpra as regras de `AGENTS.md` não deve ser enviada ao repositório. Se um método depender diretamente da interface ou de uma API de plataforma, extraia sua lógica para um componente injetável e testável antes de concluir a implementação.

## Comunicação BLE no Android

O primeiro incremento real está implementado por `AndroidBluetoothLowEnergyTransport` e permite:

1. Solicitar as permissões necessárias.
2. Verificar se Bluetooth está disponível e ligado.
3. Buscar periféricos BLE.
4. Retornar os dispositivos encontrados para o controlador.
5. Abrir e encerrar uma conexão GATT sem enviar comandos proprietários.

A descoberta de serviços e características será o próximo incremento e deverá ser apresentada em uma área de diagnóstico. Comandos do relógio só devem ser adicionados depois de confirmados e registrados em `D20_PROTOCOL.md`.

Não force vínculo com `CreateBond`. Uma conexão GATT não exige necessariamente que o periférico apareça na lista de dispositivos pareados do Android. O vínculo só deve ser solicitado quando uma característica confirmada exigir autenticação.

## Tratamento de estado

Operações BLE são assíncronas e sujeitas a interrupções. A interface deve representar pelo menos:

- Bluetooth indisponível ou desligado.
- Permissão pendente ou negada.
- Busca em andamento, concluída ou cancelada.
- Dispositivo selecionado.
- Conexão em andamento, ativa ou encerrada.
- Timeout e perda inesperada de conexão.

Não permita duas buscas ou duas operações GATT incompatíveis ao mesmo tempo.

## Diagnóstico Android

### XA0129 durante a implantação

O projeto incorpora as DLLs no APK de Debug para evitar falhas do Fast Deployment em `files/.__override__`. Se o erro reaparecer:

1. Pare a sessão de depuração.
2. Limpe o projeto.
3. Remova a instalação de desenvolvimento do aparelho.
4. Compile e implante novamente.
5. Confirme que `EmbedAssembliesIntoApk` continua habilitado para Debug Android.

### Aplicativo fecha na tela inicial

Consulte primeiro a exceção completa no Logcat. Mensagens posteriores, como avisos sobre `liblog`, podem ser apenas consequência da exceção original.

## Checklist antes de enviar alterações

- [ ] Todos os métodos criados ou alterados possuem testes unitários adequados.
- [ ] Todos os testes passam.
- [ ] A mudança pertence à camada correta.
- [ ] A implementação segue SOLID e depende de abstrações nos limites externos.
- [ ] Não há regras, validações, constantes ou fluxos duplicados.
- [ ] O fluxo simulado continua funcionando.
- [ ] Os destinos afetados compilam sem erros.
- [ ] Erros e cancelamentos têm comportamento visível.
- [ ] Nenhum protocolo não confirmado foi tratado como definitivo.
- [ ] README e documentos técnicos estão atualizados.
- [ ] `git status` não contém artefatos, credenciais ou arquivos locais.
