using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;

namespace ADE.Reporting.Pdf;

public static class PdfReportManager
{
    public static void Generate(
        ReportModel model,
        string destinationFile)
    {
        QuestPDF.Settings.License =
            LicenseType.Community;

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(50);
                page.DefaultTextStyle(x => x
                    .FontSize(11)
                    .FontFamily(Fonts.Calibri)
                    .FontColor(Colors.Black)
                    .LineHeight(1.5f));

                // CABEÇALHO COM GRADIENTE MELHORADO
                page.Header().Column(column =>
                {
                    column.Item().Background(Colors.Blue.Darken3).Padding(25).Row(row =>
                    {
                        // Logo placeholder (left side)
                        row.ConstantItem(100).Height(100).Background(Colors.White)
                            .AlignCenter().AlignMiddle()
                            .Border(2).BorderColor(Colors.Grey.Lighten2).CornerRadius(8)
                            .Column(logoCol =>
                            {
                                string logoPath = Path.Combine(Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory) ?? "", "ADE.UI", "Resources", "logo.png");
                                if (!File.Exists(logoPath))
                                {
                                    logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "logo.png");
                                }
                                if (File.Exists(logoPath))
                                {
                                    try
                                    {
                                        logoCol.Item().Image(logoPath);
                                    }
                                    catch
                                    {
                                        logoCol.Item().Text("ADE").FontSize(32).Bold().FontColor(Colors.Blue.Darken3);
                                    }
                                }
                                else
                                {
                                    logoCol.Item().Text("ADE").FontSize(32).Bold().FontColor(Colors.Blue.Darken3);
                                }
                            });

                        // Title section (center)
                        row.RelativeItem().PaddingLeft(25).Column(col =>
                        {
                            col.Item().Text("RELATÓRIO DE CAPTURA TÉCNICA").FontSize(28).Bold().FontColor(Colors.White);
                            col.Item().PaddingTop(12).Text($"Identificador: {model.CaseId}").FontSize(16).FontColor(Colors.Grey.Lighten3);
                            col.Item().Text($"Coleta de provas digitais com validade jurídica").FontSize(14).FontColor(Colors.Grey.Lighten3);
                            col.Item().PaddingTop(12).Text($"Responsável: {model.OfficerName} | {model.UnitName}").FontSize(13).FontColor(Colors.White);
                        });
                    });

                    column.Item().PaddingTop(25).Row(row =>
                    {
                        row.RelativeItem().Border(2).BorderColor(Colors.Grey.Lighten2).Padding(15).Background(Colors.Blue.Darken2).CornerRadius(8)
                            .Text($"BO: {model.BoNumber}").FontSize(14).FontColor(Colors.White).Bold();
                        row.ConstantItem(30);
                        row.RelativeItem().Border(2).BorderColor(Colors.Grey.Lighten2).Padding(15).Background(Colors.Blue.Darken2).CornerRadius(8)
                            .Text($"Procedimento: {model.ProcedureNumber}").FontSize(14).FontColor(Colors.White).Bold();
                    });
                });

                page.Content().Column(column =>
                {
                    column.Spacing(20);

                    // SEÇÃO DE INTRODUÇÃO
                    column.Item().Column(col =>
                    {
                        col.Item().Text("1. INTRODUÇÃO").FontSize(18).Bold().FontColor(Colors.Blue.Darken3);
                        col.Item().PaddingTop(10).Text(
                            "O presente Relatório Técnico de Captura de Evidências Digitais documenta as operações realizadas durante uma sessão de coleta " +
                            "conduzida pelo sistema ADE – Ata Digital de Evidências, registrando de forma cronológica os eventos ocorridos, os arquivos " +
                            "produzidos, seus metadados e os respectivos mecanismos de verificação de integridade.")
                            .FontSize(11).Justify();
                        col.Item().PaddingTop(10).Text(
                            "O ADE consiste em uma ferramenta destinada à documentação técnica de evidências digitais, permitindo registrar capturas de tela, " +
                            "gravações de vídeo, importação de arquivos e demais elementos digitais relevantes para procedimentos administrativos, investigativos ou judiciais.")
                            .FontSize(11).Justify();
                        col.Item().PaddingTop(10).Text(
                            "Durante toda a sessão são registrados automaticamente os eventos executados pelo usuário e pelo sistema, incluindo horários, " +
                            "identificação dos arquivos produzidos, métodos empregados na coleta e resumos criptográficos (hashes), preservando o histórico das operações realizadas.")
                            .FontSize(11).Justify();
                        col.Item().PaddingTop(10).Text(
                            "O objetivo deste relatório é fornecer documentação técnica suficiente para permitir que terceiros verifiquem a integridade dos arquivos " +
                            "apresentados, preservando a rastreabilidade da coleta e contribuindo para a adequada análise da evidência digital.")
                            .FontSize(11).Justify();
                        col.Item().PaddingTop(10).Text(
                            "Este documento possui natureza exclusivamente técnica e descritiva, não realizando juízo de valor sobre o conteúdo coletado, " +
                            "limitando-se a registrar os elementos produzidos durante a sessão.")
                            .FontSize(11).Justify();
                    });

                    // SEÇÃO DE FUNDAMENTAÇÃO TÉCNICA
                    column.Item().Column(col =>
                    {
                        col.Item().Text("2. FUNDAMENTAÇÃO TÉCNICA").FontSize(18).Bold().FontColor(Colors.Blue.Darken3);
                        col.Item().PaddingTop(10).Text(
                            "A documentação das evidências foi elaborada observando boas práticas internacionalmente reconhecidas para identificação, coleta, " +
                            "aquisição, preservação e documentação de evidências digitais, especialmente aquelas previstas na ABNT NBR ISO/IEC 27037:2013, " +
                            "que estabelece diretrizes para o tratamento de evidências potencialmente relevantes para investigações.")
                            .FontSize(11).Justify();
                        col.Item().PaddingTop(10).Text(
                            "O sistema registra automaticamente informações técnicas capazes de permitir a posterior verificação da integridade dos arquivos produzidos, " +
                            "incluindo: identificação única do caso; registro cronológico dos eventos; identificação do método de captura; metadados disponíveis; " +
                            "identificação dos arquivos gerados; cálculo dos códigos HASH SHA-256 e SHA-512; histórico das operações realizadas durante a sessão.")
                            .FontSize(11).Justify();
                        col.Item().PaddingTop(10).Text(
                            "Os códigos HASH consistem em resumos criptográficos calculados sobre o conteúdo dos arquivos, funcionando como identificadores matemáticos únicos. " +
                            "Qualquer alteração posterior no conteúdo do arquivo produzirá um valor HASH diferente daquele registrado neste relatório, " +
                            "permitindo a verificação objetiva de sua integridade.")
                            .FontSize(11).Justify();
                        col.Item().PaddingTop(10).Text(
                            "Os registros produzidos pelo ADE possuem caráter documental e técnico, possibilitando a reprodução independente da conferência dos arquivos apresentados.")
                            .FontSize(11).Justify();
                    });

                    // SEÇÃO DE FUNDAMENTAÇÃO JURÍDICA
                    column.Item().Column(col =>
                    {
                        col.Item().Text("3. FUNDAMENTAÇÃO JURÍDICA").FontSize(18).Bold().FontColor(Colors.Blue.Darken3);
                        col.Item().PaddingTop(10).Text(
                            "A documentação das evidências observa os princípios aplicáveis à preservação e rastreabilidade dos vestígios digitais previstos na legislação brasileira.")
                            .FontSize(11).Justify();
                        col.Item().PaddingTop(10).Text(
                            "No âmbito processual penal, a cadeia de custódia encontra disciplina nos artigos 158-A a 158-F do Código de Processo Penal, " +
                            "compreendendo o conjunto de procedimentos destinados à identificação, coleta, registro, acondicionamento, preservação, transporte, " +
                            "armazenamento e documentação dos vestígios.")
                            .FontSize(11).Justify();
                        col.Item().PaddingTop(10).Text(
                            "Embora a natureza e a utilização das evidências devam ser analisadas de acordo com as peculiaridades de cada procedimento, " +
                            "o registro técnico dos eventos, dos metadados e dos resumos criptográficos contribui para a demonstração da integridade dos arquivos apresentados.")
                            .FontSize(11).Justify();
                        col.Item().PaddingTop(10).Text(
                            "No âmbito processual civil, o artigo 369 do Código de Processo Civil assegura às partes o direito de empregar todos os meios legais " +
                            "e moralmente legítimos aptos a demonstrar a verdade dos fatos, cabendo ao julgador valorar as provas produzidas em conjunto com os demais " +
                            "elementos constantes dos autos.")
                            .FontSize(11).Justify();
                        col.Item().PaddingTop(10).Text(
                            "A autenticidade dos arquivos documentados neste relatório pode ser aferida por meio da conferência independente dos códigos HASH registrados, " +
                            "bem como pela análise dos metadados e do histórico dos eventos documentados.")
                            .FontSize(11).Justify();
                        col.Item().PaddingTop(10).Text(
                            "Este relatório constitui instrumento técnico de documentação da coleta realizada, sem substituir a atividade pericial eventualmente determinada " +
                            "pela autoridade competente nem estabelecer presunção absoluta de autenticidade ou veracidade do conteúdo registrado.")
                            .FontSize(11).Justify();
                    });

                    // SEÇÃO DE VERIFICAÇÃO DE INTEGRIDADE
                    column.Item().Column(col =>
                    {
                        col.Item().Text("4. VERIFICAÇÃO DE INTEGRIDADE").FontSize(18).Bold().FontColor(Colors.Blue.Darken3);
                        col.Item().PaddingTop(10).Text(
                            "A confiabilidade das informações apresentadas neste relatório depende da preservação integral deste documento e dos arquivos que o acompanham.")
                            .FontSize(11).Justify();
                        col.Item().PaddingTop(10).Text(
                            "Cada evidência possui códigos HASH calculados pelos algoritmos SHA-256 e SHA-512, permitindo que qualquer interessado realize a verificação independente da integridade dos arquivos.")
                            .FontSize(11).Justify();
                        col.Item().PaddingTop(10).Text(
                            "Para validação, basta calcular novamente o HASH do arquivo utilizando qualquer ferramenta compatível e comparar o resultado obtido com o valor constante neste relatório.")
                            .FontSize(11).Justify();
                        col.Item().PaddingTop(10).Text(
                            "Caso o valor calculado seja diferente daquele registrado, deverá ser considerada a existência de alteração posterior no arquivo, comprometendo sua integridade em relação ao registro originalmente documentado.")
                            .FontSize(11).Justify();
                        col.Item().PaddingTop(10).Text(
                            "Recomenda-se que a verificação seja realizada antes da utilização das evidências em procedimentos administrativos, disciplinares, investigativos ou judiciais.")
                            .FontSize(11).Justify();
                    });

                    // SEÇÃO DE INTEGRIDADE COM DESTAQUE
                    column.Item().Padding(20).Background(Colors.Blue.Lighten1).Border(3).BorderColor(Colors.Blue.Darken2).Column(col =>
                    {
                        col.Item().Text("CÓDIGOS HASH DO CASO").FontSize(18).Bold().FontColor(Colors.Blue.Darken3);
                        col.Item().PaddingTop(12).Text($"Master SHA256: {model.MasterSha256}").FontSize(11).FontColor(Colors.Blue.Darken2);
                        col.Item().Text($"Master SHA512: {model.MasterSha512}").FontSize(11).FontColor(Colors.Blue.Darken2);
                    });

                    // TABELA DE EVIDÊNCIAS
                    column.Item().Column(col =>
                    {
                        col.Item().Text("ARQUIVOS REGISTRADOS").FontSize(20).Bold().FontColor(Colors.Blue.Darken3);

                        col.Item().PaddingTop(15).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(50); // #
                                columns.RelativeColumn();    // Arquivo
                                columns.ConstantColumn(100);  // Método
                                columns.ConstantColumn(80);  // Tipo
                                columns.ConstantColumn(120); // Data
                                columns.RelativeColumn();    // SHA256
                                columns.RelativeColumn();    // SHA512
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Blue.Darken2).Padding(10)
                                    .Text("#").FontSize(11).Bold().FontColor(Colors.White);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(10)
                                    .Text("Arquivo").FontSize(11).Bold().FontColor(Colors.White);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(10)
                                    .Text("Método").FontSize(11).Bold().FontColor(Colors.White);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(10)
                                    .Text("Tipo").FontSize(11).Bold().FontColor(Colors.White);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(10)
                                    .Text("Data").FontSize(11).Bold().FontColor(Colors.White);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(10)
                                    .Text("SHA256").FontSize(11).Bold().FontColor(Colors.White);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(10)
                                    .Text("SHA512").FontSize(11).Bold().FontColor(Colors.White);
                            });

                            int index = 1;
                            foreach (var evidence in model.Evidences)
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8)
                                    .Text(index.ToString()).FontSize(11);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8)
                                    .Text(evidence.FileName).FontSize(11);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8)
                                    .Text(evidence.CollectionMethod).FontSize(11);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8)
                                    .Text(evidence.EvidenceType).FontSize(11);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8)
                                    .Text(evidence.CollectedAtUtc.ToString("dd/MM/yyyy HH:mm")).FontSize(11);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8)
                                    .Text(evidence.Sha256).FontSize(9);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8)
                                    .Text(evidence.Sha512).FontSize(9);
                                index++;
                            }
                        });
                    });

                    // SEÇÃO DE TIMELINE
                    column.Item().Column(col =>
                    {
                        col.Item().Text("5. CADEIA DE CUSTÓDIA DOCUMENTAL").FontSize(18).Bold().FontColor(Colors.Blue.Darken2);
                        col.Item().PaddingTop(10).Text(
                            "Durante a sessão são registrados automaticamente os eventos relevantes para reconstrução das operações realizadas.")
                            .FontSize(11).Justify();
                        col.Item().PaddingTop(10).Text(
                            "Entre eles podem ser registrados: criação do caso; início da sessão; capturas de tela; gravações de vídeo; importação de arquivos; " +
                            "exportação de evidências; geração do relatório; encerramento da sessão.")
                            .FontSize(11).Justify();
                        col.Item().PaddingTop(10).Text(
                            "Cada evento recebe identificação temporal e integra o histórico cronológico do caso, permitindo a reconstrução das operações documentadas durante a coleta.")
                            .FontSize(11).Justify();

                        col.Item().PaddingTop(15).Text("HISTÓRICO DE EVENTOS").FontSize(16).Bold().FontColor(Colors.Blue.Darken2);

                        col.Item().PaddingTop(12).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(110); // Timestamp
                                columns.ConstantColumn(90);  // Evento
                                columns.RelativeColumn();    // Descrição
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(8)
                                    .Text("Timestamp").FontSize(10).Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(8)
                                    .Text("Evento").FontSize(10).Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(8)
                                    .Text("Descrição").FontSize(10).Bold();
                            });

                            foreach (var item in model.Timeline)
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6)
                                    .Text(item.TimestampUtc.ToString("dd/MM/yyyy HH:mm:ss")).FontSize(10);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6)
                                    .Text(item.EventType).FontSize(10);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6)
                                    .Text(item.Description).FontSize(10);
                            }
                        });
                    });

                    // SEÇÃO DE ANEXOS (IMAGENS)
                    var imageEvidences = model.Evidences.Where(e =>
                        e.EvidenceType.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
                        e.EvidenceType.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
                        e.EvidenceType.Equals(".jpeg", StringComparison.OrdinalIgnoreCase)).ToList();

                    if (imageEvidences.Any())
                    {
                        column.Item().Column(col =>
                        {
                            col.Item().Text("ANEXO: IMAGENS DE TELA").FontSize(18).Bold().FontColor(Colors.Blue.Darken2);
                            col.Item().Text("Seguem as imagens registradas durante a sessão:").FontSize(11).Italic();

                            foreach (var evidence in imageEvidences)
                            {
                                string imagePath = Path.Combine(model.CaseFolder, evidence.RelativePath);

                                col.Item().PaddingTop(12).Border(2).BorderColor(Colors.Grey.Lighten1).Padding(12).Column(imgCol =>
                                {
                                    imgCol.Item().Row(row =>
                                    {
                                        // Try to load and display the actual image
                                        if (File.Exists(imagePath))
                                        {
                                            try
                                            {
                                                row.ConstantItem(220).Height(165).Border(2).BorderColor(Colors.Grey.Lighten2)
                                                    .Image(imagePath);
                                            }
                                            catch
                                            {
                                                // Fallback to placeholder if image loading fails
                                                row.ConstantItem(220).Height(165).Background(Colors.Grey.Lighten3)
                                                    .AlignCenter().AlignMiddle()
                                                    .Text("IMG").FontSize(14).Bold().FontColor(Colors.Grey.Darken1);
                                            }
                                        }
                                        else
                                        {
                                            row.ConstantItem(220).Height(165).Background(Colors.Grey.Lighten3)
                                                .AlignCenter().AlignMiddle()
                                                .Text("IMG").FontSize(14).Bold().FontColor(Colors.Grey.Darken1);
                                        }

                                        row.RelativeItem().PaddingLeft(18).Column(infoCol =>
                                        {
                                            infoCol.Item().Text($"Arquivo: {evidence.FileName}").FontSize(11).Bold();
                                            infoCol.Item().Text($"Registrado em: {evidence.CollectedAtUtc:dd/MM/yyyy HH:mm:ss}").FontSize(10);
                                            infoCol.Item().PaddingTop(8).Text($"HASH SHA512: {evidence.Sha512}").FontSize(8);
                                            infoCol.Item().Text($"HASH SHA256: {evidence.Sha256}").FontSize(8);
                                            infoCol.Item().PaddingTop(8).Text($"Método: {evidence.CollectionMethod}").FontSize(10);
                                            infoCol.Item().Text($"Caminho: {evidence.RelativePath}").FontSize(10).Italic();
                                        });
                                    });
                                });
                            }
                        });
                    }

                    // SEÇÃO DE LIMITAÇÕES
                    column.Item().Column(col =>
                    {
                        col.Item().Text("6. LIMITAÇÕES DO REGISTRO").FontSize(18).Bold().FontColor(Colors.Blue.Darken2);
                        col.Item().PaddingTop(10).Text(
                            "O ADE registra tecnicamente os elementos apresentados ao sistema durante a sessão de captura.")
                            .FontSize(11).Justify();
                        col.Item().PaddingTop(10).Text(
                            "A ferramenta não altera, interpreta, certifica ou valida o conteúdo das informações exibidas ao usuário, limitando-se à documentação técnica da coleta realizada.")
                            .FontSize(11).Justify();
                        col.Item().PaddingTop(10).Text(
                            "A análise da origem das informações, da autoria do conteúdo, de sua veracidade, completude, contexto ou relevância jurídica deverá ser realizada pela autoridade competente ou por profissional habilitado, conforme a natureza do procedimento.")
                            .FontSize(11).Justify();
                        col.Item().PaddingTop(10).Text(
                            "Da mesma forma, a aceitação deste relatório como elemento de prova dependerá da legislação aplicável ao caso concreto, da análise conjunta das demais provas produzidas e do convencimento da autoridade responsável pela decisão.")
                            .FontSize(11).Justify();
                    });

                    // SEÇÃO DE DECLARAÇÃO TÉCNICA
                    column.Item().Column(col =>
                    {
                        col.Item().Text("7. DECLARAÇÃO TÉCNICA").FontSize(18).Bold().FontColor(Colors.Blue.Darken2);
                        col.Item().PaddingTop(10).Text(
                            "Este relatório foi gerado automaticamente pelo sistema ADE – Ata Digital de Evidências, a partir dos registros produzidos durante a sessão de captura.")
                            .FontSize(11).Justify();
                        col.Item().PaddingTop(10).Text(
                            "As informações aqui constantes refletem exclusivamente os eventos documentados pelo sistema, os arquivos produzidos ou importados, seus metadados disponíveis e os respectivos resumos criptográficos calculados no momento do registro.")
                            .FontSize(11).Justify();
                        col.Item().PaddingTop(10).Text(
                            "O documento possui finalidade de documentação técnica da coleta realizada, permitindo a verificação independente da integridade dos arquivos apresentados e a reconstrução cronológica das operações executadas durante a sessão.")
                            .FontSize(11).Justify();
                        col.Item().PaddingTop(10).Text(
                            "Não constitui certificação de autenticidade material do conteúdo capturado, nem substitui perícia oficial ou particular eventualmente realizada, servindo como instrumento de preservação, organização e documentação de evidências digitais.")
                            .FontSize(11).Justify();
                    });

                    // SEÇÃO DE CONSIDERAÇÕES FINAIS
                    column.Item().Column(col =>
                    {
                        col.Item().Text("8. CONSIDERAÇÕES FINAIS").FontSize(18).Bold().FontColor(Colors.Blue.Darken2);
                        col.Item().PaddingTop(10).Text(
                            "As evidências relacionadas neste relatório permanecem vinculadas aos respectivos identificadores, registros temporais e códigos HASH apresentados neste documento.")
                            .FontSize(11).Justify();
                        col.Item().PaddingTop(10).Text(
                            "A manutenção da integridade dos arquivos originais e deste relatório é condição essencial para a preservação de sua rastreabilidade documental.")
                            .FontSize(11).Justify();
                        col.Item().PaddingTop(10).Text(
                            "Sempre que possível, recomenda-se que os arquivos sejam armazenados em mídia somente leitura, acompanhados deste relatório, do histórico de eventos e do arquivo de manifesto do caso, permitindo futura verificação independente por qualquer interessado.")
                            .FontSize(11).Justify();
                    });
                });

                // RODAPÉ
                page.Footer().AlignCenter().PaddingTop(10).Column(col =>
                {
                    col.Item().AlignCenter().Text("ADE – Ata Digital de Evidências")
                        .FontSize(9).Bold().FontColor(Colors.Grey.Darken1);
                    col.Item().PaddingTop(3).AlignCenter().Text("Ferramenta destinada à documentação técnica, preservação e organização de evidências digitais, fundamentada em boas práticas de computação forense, rastreabilidade documental e verificação independente de integridade.")
                        .FontSize(7).FontColor(Colors.Grey.Darken1);
                });
            });
        })
        .GeneratePdf(destinationFile);
    }
}