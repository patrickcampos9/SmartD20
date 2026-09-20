# Guia de desenvolvimento

Este guia complementa as regras obrigatórias de `AGENTS.md` e descreve o fluxo recomendado para trabalhar no SmartD20.

## Preparação do ambiente

Instale o Visual Studio com a carga de trabalho .NET MAUI, o SDK do .NET 10 e o SDK Android API 36. Para testes em aparelho físico, habilite a depuração USB e autorize o computador no Android.

Abra `D20Mobile.slnx`, selecione o projeto `D20Mobile` e escolha o dispositivo de destino.

## Fluxo de desenvolvimento

1. Confirme que a branch local está atualizada e sem alterações inesperadas.
2. Faça a mudança na camada responsável pelo comportamento.
3. Use o serviço simulado para validar fluxos independentes do relógio.
4. Compile os destinos afetados.
5. Execute no Android quando houver mudança em inicialização, XAML ou integração de plataforma.
6. Atualize a documentação relacionada.
7. Revise os arquivos antes do commit para evitar artefatos e dados locais.

## Como adicionar a comunicação BLE real

A implementação real deve cumprir `ID20ConnectionService`. O primeiro incremento deverá apenas:

1. Solicitar as permissões necessárias.
2. Verificar se Bluetooth está disponível e ligado.
3. Buscar periféricos BLE.
4. Retornar os dispositivos encontrados para o controlador.
5. Permitir conexão e desconexão sem ainda enviar comandos proprietários.

A descoberta de serviços e características deve ser apresentada em uma área de diagnóstico. Comandos do relógio só devem ser adicionados depois de confirmados e registrados em `D20_PROTOCOL.md`.

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

- [ ] A mudança pertence à camada correta.
- [ ] O fluxo simulado continua funcionando.
- [ ] Os destinos afetados compilam sem erros.
- [ ] Erros e cancelamentos têm comportamento visível.
- [ ] Nenhum protocolo não confirmado foi tratado como definitivo.
- [ ] README e documentos técnicos estão atualizados.
- [ ] `git status` não contém artefatos, credenciais ou arquivos locais.

