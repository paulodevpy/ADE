using System;
using System.IO;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;

namespace ADE.Reporting.Pdf;

public static class PdfReportManager
{
    // Paleta institucional (forense/policial)
    private const string Navy = "#1a365d";   // Azul-marinho principal
    private const string Blue = "#2d5a87";   // Azul médio (faixas/cabeçalhos de tabela)
    private const string Accent = "#4a90e2"; // Azul de destaque (bordas)
    private const string SoftBlue = "#e8f4f8"; // Azul claro (caixas de destaque)
    private const string PaleBlue = "#f0f7ff"; // Azul muito claro (faixas de seção)

    public static void Generate(
        ReportModel model,
        string destinationFile)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        Document.Create(document =>
        {
            // ------------------------------------------------------------
            // CAPA
            // ------------------------------------------------------------

            document.Page(page =>
            {
                page.Margin(45);

                ComposeCover(page, model);
            });

            // ------------------------------------------------------------
            // RELATÓRIO
            // ------------------------------------------------------------

            document.Page(page =>
            {
                page.Margin(40);

                ComposeHeader(page, model);

                ComposeFooter(page);

                page.Content()
                    .PaddingVertical(10)
                    .Column(column =>
                    {
                        ComposeExecutiveSummary(column, model);

                        ComposeContent(column, model);
                    });
            });
           
        }).GeneratePdf(destinationFile);
    }

    private static void ComposeCover(
        PageDescriptor page,
        ReportModel model)
    {
        page.Content()
            .PaddingTop(40)
            .Column(column =>
            {
                column.Item()
                    .AlignCenter()
                    .Width(120)
                    .Height(120)
                    .Element(container =>
                    {
                        var logo = ResolveLogoPath();

                        if (!string.IsNullOrWhiteSpace(logo))
                        {
                            container.Image(logo).FitArea();
                        }
                    });

                column.Item()
                    .PaddingTop(25)
                    .AlignCenter()
                    .Text("ATA DIGITAL DE EVIDÊNCIAS")
                    .FontSize(24)
                    .Bold()
                    .FontColor(Navy);

                column.Item()
                    .PaddingTop(6)
                    .AlignCenter()
                    .Text("Relatório Técnico de Coleta de Evidências Digitais")
                    .FontSize(12)
                    .FontColor(Blue);

                column.Item()
                    .PaddingTop(35)
                    .LineHorizontal(1)
                    .LineColor(Accent);

                column.Item()
                    .PaddingTop(25)
                    .AlignCenter()
                    .Text(model.CaseId)
                    .FontSize(20)
                    .Bold();

                column.Item()
                    .PaddingTop(30)
                    .Column(info =>
                    {
                        CoverField(info, "BO", model.BoNumber);

                        CoverField(info, "PROCEDIMENTO", model.ProcedureNumber);

                        CoverField(info, "UNIDADE", model.UnitName);

                        CoverField(info, "RESPONSÁVEL", model.OfficerName);

                        CoverField(
                            info,
                            "GERADO EM",
                            model.GeneratedAtUtc.ToString("dd/MM/yyyy HH:mm:ss UTC"));
                    });

                column.Item()
                    .PaddingTop(40);

                column.Item()
                    .AlignCenter()
                    .Text("ADE – Ata Digital de Evidências")
                    .FontSize(10)
                    .Bold();

                column.Item()
                    .AlignCenter()
                    .Text("Versão 1.0")
                    .FontSize(9);
            });
    }

    private static void CoverField(
        ColumnDescriptor column,
        string title,
        string? value)
    {
        column.Item()
            .PaddingBottom(12)
            .Column(item =>
            {
                item.Item()
                    .Text(title)
                    .FontSize(8)
                    .Bold()
                    .FontColor(Blue);

                item.Item()
                    .PaddingTop(2)
                    .Text(value ?? "-")
                    .FontSize(11);

                item.Item()
                    .PaddingTop(4)
                    .LineHorizontal(0.3f)
                    .LineColor(Colors.Grey.Lighten2);
            });
    }

    private static void ComposeExecutiveSummary(
        ColumnDescriptor column,
        ReportModel model)
    {
        SectionHeader(column, "RESUMO EXECUTIVO");

        int screenshots =
            model.Evidences.Count(x =>
                x.EvidenceType.Contains("png", StringComparison.OrdinalIgnoreCase)
                || x.EvidenceType.Contains("jpg", StringComparison.OrdinalIgnoreCase));

        int videos =
            model.Evidences.Count(x =>
                x.EvidenceType.Contains("mp4", StringComparison.OrdinalIgnoreCase));

        column.Item()
            .PaddingTop(10)
            .Row(row =>
            {
                SummaryCard(
                    row.RelativeItem(),
                    "Arquivos",
                    model.Evidences.Count.ToString());

                row.ConstantItem(8);

                SummaryCard(
                    row.RelativeItem(),
                    "Capturas",
                    screenshots.ToString());

                row.ConstantItem(8);

                SummaryCard(
                    row.RelativeItem(),
                    "Vídeos",
                    videos.ToString());

                row.ConstantItem(8);

                SummaryCard(
                    row.RelativeItem(),
                    "Eventos",
                    model.Timeline.Count.ToString());
            });

        column.Item()
            .PaddingTop(18)
            .Background(PaleBlue)
            .Border(1)
            .BorderColor(Accent)
            .Padding(12)
            .Column(hash =>
            {
                hash.Item()
                    .Text("MASTER SHA-256")
                    .Bold()
                    .FontColor(Navy);

                hash.Item()
                    .Text(model.MasterSha256)
                    .FontFamily(Fonts.CourierNew)
                    .FontSize(7);

                hash.Item()
                    .PaddingTop(10)
                    .Text("MASTER SHA-512")
                    .Bold()
                    .FontColor(Navy);

                hash.Item()
                    .Text(model.MasterSha512)
                    .FontFamily(Fonts.CourierNew)
                    .FontSize(6);
            });
    }

    private static void SummaryCard(
        IContainer container,
        string title,
        string value)
    {
        container
            .Background(PaleBlue)
            .Border(1)
            .BorderColor(Accent)
            .Padding(12)
            .Column(column =>
            {
                column.Item()
                    .AlignCenter()
                    .Text(value)
                    .FontSize(20)
                    .Bold()
                    .FontColor(Navy);

                column.Item()
                    .PaddingTop(5)
                    .AlignCenter()
                    .Text(title)
                    .FontSize(8)
                    .FontColor(Colors.Grey.Darken2);
            });
    }

    // ---------------------------------------------------------------------
    // CABEÇALHO
    // ---------------------------------------------------------------------   
    private static void ComposeHeader(PageDescriptor page, ReportModel model)
    {
        page.Header().Column(column =>
        {
            column.Item().Row(row =>
            {
                row.ConstantItem(46)
                    .Height(46)
                    .Element(container =>
                    {
                        var logo = ResolveLogoPath();

                        if (!string.IsNullOrWhiteSpace(logo))
                        {
                            container.Image(logo).FitArea();
                        }
                        else
                        {
                            container.AlignCenter()
                                .AlignMiddle()
                                .Text("ADE")
                                .Bold()
                                .FontSize(14)
                                .FontColor(Navy);
                        }
                    });

                row.RelativeItem()
                    .PaddingLeft(10)
                    .Column(col =>
                    {
                        col.Item()
                            .Text("ADE – Ata Digital de Evidências")
                            .FontSize(14)
                            .Bold()
                            .FontColor(Navy);

                        col.Item()
                            .Text($"Caso {model.CaseId}")
                            .FontSize(8)
                            .FontColor(Colors.Grey.Darken2);
                    });
            });

            column.Item()
                .PaddingTop(5)
                .LineHorizontal(0.6f)
                .LineColor(Colors.Grey.Lighten2);
        });
    }

    // ---------------------------------------------------------------------
    // CONTEÚDO
    // ---------------------------------------------------------------------
    private static void ComposeContent(ColumnDescriptor column, ReportModel model)    
    {
        column.Spacing(20);
            column.Spacing(20);

            // 1. INTRODUÇÃO
            Section(column, "1. INTRODUÇÃO", body =>
            {
                Paragraph(body,
                    "O presente relatório foi gerado automaticamente pelo sistema ADE – Ata Digital de Evidências, destinado ao registro, organização e documentação técnica de evidências digitais produzidas ou incorporadas durante uma sessão de coleta.");

                Paragraph(body,
                    "O documento reúne os eventos registrados cronologicamente, os arquivos produzidos ou importados, seus metadados disponíveis e os respectivos identificadores criptográficos SHA-256 e SHA-512, permitindo a verificação independente da integridade de cada evidência.");

                Paragraph(body,
                    "Sua finalidade é preservar a rastreabilidade da coleta e fornecer documentação técnica destinada à instrução de procedimentos administrativos, investigativos ou judiciais.");
            });

            // 2. FUNDAMENTAÇÃO TÉCNICA
            Section(column, "2. FUNDAMENTAÇÃO TÉCNICA", body =>
            {
                Paragraph(body,
                    "O ADE registra automaticamente os eventos ocorridos durante a sessão de coleta, associando cada evidência aos respectivos metadados disponíveis e aos identificadores criptográficos calculados no momento do registro.");

                Paragraph(body,
                    "Os códigos HASH SHA-256 e SHA-512 funcionam como identificadores matemáticos únicos, permitindo a verificação objetiva da integridade dos arquivos documentados.");

                Paragraph(body,
                    "Qualquer alteração posterior produzirá identificadores distintos, possibilitando conferência independente mediante ferramentas compatíveis.");
            });

            // 3. FUNDAMENTAÇÃO JURÍDICA
            Section(column, "3. FUNDAMENTAÇÃO JURÍDICA", body =>
            {
                Paragraph(body,
                    "A documentação técnica das evidências observa os princípios da autenticidade, integridade, rastreabilidade e cadeia de custódia previstos na legislação brasileira.");

                Paragraph(body,
                    "Constituem fundamentos normativos relevantes os artigos 158-A a 158-F do Código de Processo Penal, o artigo 369 do Código de Processo Civil e a Medida Provisória nº 2.200-2/2001, sem prejuízo da legislação específica aplicável ao caso concreto.");

                Paragraph(body,
                    "Este relatório possui natureza exclusivamente documental e técnica, não substituindo eventual perícia oficial ou particular.");
            });

            // 4. VERIFICAÇÃO DE INTEGRIDADE
            Section(column, "4. VERIFICAÇÃO DE INTEGRIDADE", body =>
            {
                Paragraph(body,
                    "Cada evidência registrada possui identificadores criptográficos SHA-256 e SHA-512 calculados automaticamente durante sua inclusão na ata digital.");

                Paragraph(body,
                    "A conferência independente desses identificadores permite verificar se o conteúdo permanece íntegro em relação ao momento do registro.");

                Paragraph(body,
                    "A divergência entre os valores constantes neste relatório e aqueles obtidos posteriormente indica alteração do arquivo originalmente documentado.");
            });

            // ---------------------------------------------------------------------
            // IDENTIFICADORES CRIPTOGRÁFICOS DO CASO
            // ---------------------------------------------------------------------
            column.Item()
                .Column(col =>
                {
                    SectionHeader(col, "IDENTIFICADORES CRIPTOGRÁFICOS");

                    col.Item()
                        .PaddingTop(10)
                        .Background(PaleBlue)
                        .Border(1)
                        .BorderColor(Accent)
                        .Padding(15)
                        .Column(hash =>
                        {
                            hash.Item()
                                .Text("MASTER SHA-256")
                                .FontSize(11)
                                .Bold()
                                .FontColor(Navy);

                            hash.Item()
                                .PaddingTop(4)
                                .Text(model.MasterSha256)
                                .FontFamily(Fonts.CourierNew)
                                .FontSize(8);

                            hash.Item()
                                .PaddingTop(12)
                                .Text("MASTER SHA-512")
                                .FontSize(11)
                                .Bold()
                                .FontColor(Navy);

                            hash.Item()
                                .PaddingTop(4)
                                .Text(model.MasterSha512)
                                .FontFamily(Fonts.CourierNew)
                                .FontSize(7);
                        });
                });

            // ---------------------------------------------------------------------
            // ARQUIVOS REGISTRADOS
            // ---------------------------------------------------------------------
            column.Item().Column(col =>
            {
                SectionHeader(col, "ARQUIVOS REGISTRADOS");

                col.Item()
                    .PaddingTop(10)
                    .Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(22);   // #

                            columns.RelativeColumn(2.5f); // Arquivo

                            columns.ConstantColumn(70);   // Método

                            columns.ConstantColumn(55);   // Tipo

                            columns.ConstantColumn(82);   // Data

                            columns.RelativeColumn(2.4f); // SHA-256

                            columns.RelativeColumn(2.4f); // SHA-512
                        });

                        table.Header(header =>
                        {
                            HeaderCell(header, "#");
                            HeaderCell(header, "Arquivo");
                            HeaderCell(header, "Método");
                            HeaderCell(header, "Tipo");
                            HeaderCell(header, "Data");
                            HeaderCell(header, "SHA-256");
                            HeaderCell(header, "SHA-512");
                        });

                        int index = 1;

                        foreach (var evidence in model.Evidences)
                        {
                            string background =
                                index % 2 == 0
                                    ? PaleBlue
                                    : "#FFFFFF";

                            BodyCell(table, background, index.ToString(), 8);

                            BodyCell(
                                table,
                                background,
                                evidence.FileName,
                                8);

                            BodyCell(
                                table,
                                background,
                                evidence.CollectionMethod.Replace("_", " "),
                                8);

                            BodyCell(
                                table,
                                background,
                                evidence.EvidenceType.ToUpper(),
                                8);

                            BodyCell(
                                table,
                                background,
                                evidence.CollectedAtUtc.ToString("dd/MM/yyyy HH:mm:ss"),
                                7.5f);

                            BodyCellMono(
                                table,
                                background,
                                evidence.Sha256,
                                6);

                            BodyCellMono(
                                table,
                                background,
                                evidence.Sha512,
                                5.8f);

                            index++;
                        }
                    });
            });

            // ---------------------------------------------------------------------
            // TIMELINE DA COLETA
            // ---------------------------------------------------------------------
            column.Item().Column(col =>
            {
                SectionHeader(col, "TIMELINE DA COLETA");

                col.Item()
                    .PaddingTop(8)
                    .Column(events =>
                    {
                        int index = 1;

                        foreach (var item in model.Timeline.OrderBy(x => x.TimestampUtc))
                        {
                            string background =
                                index % 2 == 0
                                    ? PaleBlue
                                    : "#FFFFFF";

                            events.Item()
                                .Background(background)
                                .Border(0.5f)
                                .BorderColor(Colors.Grey.Lighten2)
                                .Padding(10)
                                .PaddingBottom(12)
                                .Column(card =>
                                {
                                    card.Item()
                                        .Row(row =>
                                        {
                                            row.ConstantItem(135)
                                                .Text(item.TimestampUtc.ToString("dd/MM/yyyy HH:mm:ss"))
                                                .FontFamily(Fonts.CourierNew)
                                                .FontSize(8)
                                                .FontColor(Navy);

                                            row.RelativeItem()
                                                .Text(item.EventType.Replace("_", " "))
                                                .Bold()
                                                .FontSize(10)
                                                .FontColor(Blue);
                                        });

                                    if (!string.IsNullOrWhiteSpace(item.Description))
                                    {
                                        card.Item()
                                            .PaddingTop(6)
                                            .PaddingLeft(135)
                                            .Text(item.Description)
                                            .FontSize(8)
                                            .FontColor(Colors.Grey.Darken2);
                                    }
                                });

                            index++;
                        }
                    });
            });
            
            // ANEXO: IMAGENS
            var imageEvidences = model.Evidences.Where(e =>
                e.EvidenceType.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
                e.EvidenceType.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
                e.EvidenceType.Equals(".jpeg", StringComparison.OrdinalIgnoreCase)).ToList();

            if (imageEvidences.Any())
            {
                column.Item().Column(col =>
                {
                    SectionHeader(col, "ANEXO: IMAGENS DE TELA");
                    col.Item().PaddingTop(8).Text("Seguem as imagens registradas durante a sessão:")
                        .FontSize(10).Italic();

                    foreach (var evidence in imageEvidences)
                    {
                        string imagePath =
                            Path.Combine(
                                model.CaseFolder,
                                evidence.RelativePath);

                        col.Item()
                            .PageBreak();

                        col.Item()
                            .Column(image =>
                            {
                                image.Item()
                                    .Text(evidence.FileName)
                                    .FontSize(18)
                                    .Bold()
                                    .FontColor(Navy);

                                image.Item()
                                    .PaddingTop(8)
                                    .Text($"Registrado em: {evidence.CollectedAtUtc:dd/MM/yyyy HH:mm:ss}")
                                    .FontSize(8);

                                image.Item()
                                    .Text($"Método de coleta: {evidence.CollectionMethod.Replace("_", " ")}")
                                    .FontSize(8);

                                image.Item()
                                    .PaddingTop(8)
                                    .Text("SHA-256")
                                    .Bold()
                                    .FontSize(8);

                                image.Item()
                                    .Text(FormatHash(evidence.Sha256))
                                    .FontFamily(Fonts.CourierNew)
                                    .FontSize(7);

                                image.Item()
                                    .PaddingTop(6)
                                    .Text("SHA-512")
                                    .Bold()
                                    .FontSize(8);

                                image.Item()
                                    .Text(FormatHash(evidence.Sha512))
                                    .FontFamily(Fonts.CourierNew)
                                    .FontSize(6);

                                image.Item()
                                    .PaddingTop(15)
                                    .Border(1)
                                    .BorderColor(Accent)
                                    .Padding(5)
                                    .Height(560)
                                    .Element(container =>
                                    {
                                        if (File.Exists(imagePath))
                                        {
                                            container
                                                .AlignCenter()
                                                .AlignMiddle()
                                                .Image(imagePath)
                                                .FitWidth();
                                        }
                                        else
                                        {
                                            container
                                                .AlignCenter()
                                                .AlignMiddle()
                                                .Text("Imagem não localizada")
                                                .FontSize(14)
                                                .FontColor(Colors.Grey.Darken1);
                                        }
                                    });
                            });
                    }
                });
            }

            // 6. LIMITAÇÕES
            Section(column, "6. LIMITAÇÕES DO REGISTRO", body =>
            {
                Paragraph(body, "O ADE registra tecnicamente os elementos apresentados ao sistema durante a sessão de captura.");
                Paragraph(body, "A ferramenta não altera, interpreta, certifica ou valida o conteúdo das informações exibidas ao usuário, limitando-se à documentação técnica da coleta realizada.");
                Paragraph(body, "A análise da origem das informações, da autoria do conteúdo, de sua veracidade, completude, contexto ou relevância jurídica deverá ser realizada pela autoridade competente ou por profissional habilitado, conforme a natureza do procedimento.");
                Paragraph(body, "Da mesma forma, a aceitação deste relatório como elemento de prova dependerá da legislação aplicável ao caso concreto, da análise conjunta das demais provas produzidas e do convencimento da autoridade responsável pela decisão.");
            });

            // 7. DECLARAÇÃO TÉCNICA
            Section(column, "7. DECLARAÇÃO TÉCNICA", body =>
            {
                Paragraph(body, "Este relatório foi gerado automaticamente pelo sistema ADE – Ata Digital de Evidências, a partir dos registros produzidos durante a sessão de captura.");
                Paragraph(body, "As informações aqui constantes refletem exclusivamente os eventos documentados pelo sistema, os arquivos produzidos ou importados, seus metadados disponíveis e os respectivos resumos criptográficos calculados no momento do registro.");
                Paragraph(body, "O documento possui finalidade de documentação técnica da coleta realizada, permitindo a verificação independente da integridade dos arquivos apresentados e a reconstrução cronológica das operações executadas durante a sessão.");
                Paragraph(body, "Não constitui certificação de autenticidade material do conteúdo capturado, nem substitui perícia oficial ou particular eventualmente realizada, servindo como instrumento de preservação, organização e documentação de evidências digitais.");
            });

            // 8. CONSIDERAÇÕES FINAIS
            Section(column, "8. CONSIDERAÇÕES FINAIS", body =>
            {
                Paragraph(body, "As evidências relacionadas neste relatório permanecem vinculadas aos respectivos identificadores, registros temporais e códigos HASH apresentados neste documento.");
                Paragraph(body, "A manutenção da integridade dos arquivos originais e deste relatório é condição essencial para a preservação de sua rastreabilidade documental.");
            });
        
    }

    // ---------------------------------------------------------------------
    // RODAPÉ
    // ---------------------------------------------------------------------
    private static void ComposeFooter(PageDescriptor page)
    {
        page.Footer()
            .Column(col =>
            {
                col.Item()
                    .LineHorizontal(0.4f)
                    .LineColor(Colors.Grey.Lighten2);

                col.Item()
                    .PaddingTop(3)
                    .Row(row =>
                    {
                        row.RelativeItem()
                            .Text("ADE – Ata Digital de Evidências")
                            .FontSize(7)
                            .FontColor(Colors.Grey.Darken1);

                        row.RelativeItem()
                            .AlignCenter()
                            .Text("Versão 1.0")
                            .FontSize(7)
                            .FontColor(Colors.Grey.Darken1);

                        row.RelativeItem()
                            .AlignRight()
                            .Text(txt =>
                            {
                                txt.DefaultTextStyle(x => x.FontSize(7));

                                txt.Span("Página ");

                                txt.CurrentPageNumber();

                                txt.Span(" de ");

                                txt.TotalPages();
                            });
                    });
            });
    }

    // ---------------------------------------------------------------------
    // HELPERS DE LAYOUT
    // ---------------------------------------------------------------------

    // Bloco completo: cabeçalho de seção + corpo de parágrafos
    private static void Section(ColumnDescriptor column, string title, Action<ColumnDescriptor> body)
    {
        column.Item().Column(col =>
        {
            SectionHeader(col, title);
            col.Item().PaddingTop(8).Column(body);
        });
    }

    // Cabeçalho de seção com faixa clara + borda lateral de destaque
    private static void SectionHeader(
        ColumnDescriptor column,
        string title)
    {
        column.Item()
            .PaddingTop(20)
            .PaddingBottom(5)
            .Text(title)
            .FontSize(15)
            .Bold()
            .FontColor(Navy);

        column.Item()
            .LineHorizontal(0.8f)
            .LineColor(Accent);

        column.Item()
            .PaddingBottom(5);
    }

    private static void Paragraph(
        ColumnDescriptor body,
        string text)
    {
        body.Item()
            .PaddingTop(8)
            .PaddingLeft(22)
            .PaddingRight(8)
            .Text(text)
            .FontSize(10)
            .FontColor(Colors.Grey.Darken4)
            .LineHeight(1.45f)
            .Justify();
    }

   private static void HeaderCell(
        TableCellDescriptor header,
        string text)
    {
        header.Cell()
            .Background(Navy)
            .PaddingVertical(8)
            .PaddingHorizontal(6)
            .BorderBottom(1)
            .BorderColor(Accent)
            .AlignMiddle()
            .Text(text)
            .Bold()
            .FontSize(8)
            .FontColor(Colors.White);
    }

    private static void BodyCell(
        TableDescriptor table,
        string background,
        string? value,
        float fontSize = 8)
    {
        table.Cell()
            .Background(background)
            .BorderBottom(0.3f)
            .BorderColor(Colors.Grey.Lighten2)
            .PaddingVertical(7)
            .PaddingHorizontal(6)
            .AlignMiddle()
            .Text(value ?? "")
            .FontSize(fontSize)
            .FontColor(Colors.Grey.Darken4);
    }
    private static void BodyCellMono(
        TableDescriptor table,
        string background,
        string? value,
        float fontSize = 6)
    {
        table.Cell()
            .Background(background)
            .BorderBottom(0.3f)
            .BorderColor(Colors.Grey.Lighten2)
            .PaddingVertical(7)
            .PaddingHorizontal(5)
            .AlignMiddle()
            .Text(value ?? "")
            .FontFamily(Fonts.CourierNew)
            .FontSize(fontSize)
            .FontColor(Navy);
    }

    private static void ImagePlaceholder(RowDescriptor row)
    {
        row.ConstantItem(180).Height(135).Background(SoftBlue)
            .Border(1).BorderColor(Accent)
            .AlignCenter().AlignMiddle()
            .Text("IMG").FontSize(18).Bold().FontColor(Navy);
    }

    private static string FormatHash(string? hash)
            {
                if (string.IsNullOrWhiteSpace(hash))
                    return "";

                const int block = 32;

                return string.Join(
                    Environment.NewLine,
                    Enumerable.Range(0, (hash.Length + block - 1) / block)
                        .Select(i =>
                        {
                            int start = i * block;

                            return hash.Substring(
                                start,
                                Math.Min(block, hash.Length - start));
                        }));
            }
    private static string? ResolveLogoPath()
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;

        string[] candidates =
        {
            Path.Combine(baseDir, "Resources", "logo.png"),
            Path.Combine(baseDir, "Resources", "ade.png"),

            Path.Combine(baseDir, "ADE.Reporting", "Resources", "logo.png"),
            Path.Combine(baseDir, "ADE.Reporting", "Resources", "ade.png"),

            Path.Combine(
                Directory.GetParent(baseDir)?.FullName ?? "",
                "Resources",
                "logo.png"),

            Path.Combine(
                Directory.GetParent(baseDir)?.FullName ?? "",
                "Resources",
                "ade.png"),

            Path.Combine(
                Directory.GetParent(baseDir)?.FullName ?? "",
                "ADE.Reporting",
                "Resources",
                "logo.png"),

            Path.Combine(
                Directory.GetParent(baseDir)?.FullName ?? "",
                "ADE.Reporting",
                "Resources",
                "ade.png")
        };

        foreach (var file in candidates)
        {
            if (File.Exists(file))
                return file;
        }

        return null;
    }
}