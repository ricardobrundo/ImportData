using ImportDataWebApi.Domain.Entities;
using ImportDataWebApi.Domain.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;

namespace ImportDataWebApi.Domain.Services
{
    public class ImportDataServices : IImportDataServices
    {

        private static bool StartThread { get; set; }

        private Thread thread;

        private List<Sale> lstSales { get; set; }
        private List<Salesman> lstSalesMan { get; set; }
        private List<Customer> lstCustomer { get; set; }

        private readonly CancellationTokenSource _cancellationTokenSource;

        public ImportDataServices()
        {
            this.lstSales = new List<Sale>();
            this.lstSalesMan = new List<Salesman>();
            this.lstCustomer = new List<Customer>();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        private bool ImportData()
        {
            try
            {
                
                string fileName = "importData";
                string typefile = ".dat";
                string ImportPath = @"C:\ImportData\Data\in";
                string targeImporttPath = @"C:\ImportData\Data\in\import\";

                // Use a classe Path para manipular caminhos de arquivos e diretórios.
                string sourceFile = System.IO.Path.Combine(ImportPath, fileName);
                string destFile = System.IO.Path.Combine(targeImporttPath, fileName);


                // Para copiar o conteúdo de uma pasta para um novo local:
                // Crie uma nova pasta de destino, se necessário.
                if (!System.IO.Directory.Exists(targeImporttPath))
                {
                    System.IO.Directory.CreateDirectory(targeImporttPath);
                }

                //Chamada com metodo que vai copiar arquivo para outro diretório
                CopyFileDir(sourceFile + typefile, destFile + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + typefile);


                if (System.IO.File.Exists(sourceFile + typefile))
                {
                    StreamReader objInput = new StreamReader(sourceFile + typefile, System.Text.Encoding.Default);
                    string contents = objInput.ReadToEnd().Trim().Replace(" ", "+");
                    string[] splits = System.Text.RegularExpressions.Regex.Split(contents, "\\s+", RegexOptions.None);
                    objInput.Close();

                    foreach (string s in splits)
                    {
                        var splitValueLine = s.Split('ç');
                        switch (splitValueLine[0])
                        {
                            case "001":
                                {
                                    AddSalesman(splitValueLine);
                                    break;
                                }
                            case "002":
                                {
                                    AddCustomer(splitValueLine);
                                    break;
                                }
                            case "003":
                                {
                                    AddSale(splitValueLine);
                                    break;
                                }
                            default:
                                {
                                    break;
                                }
                        }
                    }


                    //Método que analisa os dados e cria o arquivo na pasta OUT
                    AnalyzeData(sourceFile + typefile);

                    //Método que remove o arquivo da pasta IN
                    RemoveFileDir(sourceFile + typefile);

                    return true;
                }
                else
                    return false;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public bool StartService()
        {
            try
            {
                StartThread = true;
                Task.Run(() =>
                {
                    while (StartThread)
                    {
                        ImportData();
                        Thread.Sleep(TimeSpan.FromSeconds(10));
                    }
                }, _cancellationTokenSource.Token);

                return true;
            }
            catch {
                return false;
            }

        }

        public bool StopService()
        {
            try
            {

                    lstSales.Clear();
                    lstCustomer.Clear();
                    lstSalesMan.Clear();

                    //Finaliza a execução da thread (em paralelo a esse código)
                    _cancellationTokenSource.Cancel();
                    _cancellationTokenSource.Dispose();
                    StartThread = false;
                    return true;

            }
            catch
            {
                return false;
            }
        }

        #region Métodos Privados

        private void AddSale(string[] splitValueLine)
        {
            List<Item> lstItems = new List<Item>();
            if (splitValueLine.Length == 4)
            {
                Sale sale = null;
                if (lstSales.Count > 0)
                {
                    sale = (from p in lstSales
                            where p.Sale_Id == Convert.ToInt32(splitValueLine[1]) && p.SalesMan_Name == splitValueLine[3]
                            select p).FirstOrDefault();
                }

                var _items = splitValueLine[2].Substring(1, splitValueLine[2].Length - 2).Split(',');

                decimal _TotalPrice = 0;
                for (int i = 0; i < _items.Length; i++)
                {
                    var item = _items[i].Split('-');
                    if (item.Length == 3)
                    {
                        lstItems.Add(new Item()
                        {
                            Item_Id = Convert.ToInt32(item[0]),
                            Item_Qty = Convert.ToInt32(item[1]),
                            Price = (item[2].IndexOf('.') > 0 ? Convert.ToDecimal(item[2].Replace('.', ',')) : Convert.ToDecimal(item[2]))
                        });
                        
                    }

                }
                _TotalPrice += (lstItems.Count > 0 ? lstItems.Sum(c => c.Price) : 0);

                if (sale == null)
                {
                    sale = new Sale()
                    {
                        Sale_Id = Convert.ToInt32(splitValueLine[1]),
                        Items = new List<Item>(),
                        SalesMan_Name = splitValueLine[3],
                        TotalPrice = _TotalPrice
                    };

                }
                else
                {
                    lstSales.Remove(sale);
                    sale.Items.Clear();
                }

                sale.Items.AddRange(lstItems);
                lstSales.Add(sale);
            }
        }

        private void AddCustomer(string[] splitValueLine)
        {
            if (splitValueLine.Length == 4)
            {
                Customer _Customer = null;
                if (lstCustomer.Count > 0)
                {
                    _Customer = (from p in lstCustomer
                                 where p.CPNJ.Trim() == splitValueLine[1].Trim()
                                 select p).FirstOrDefault();
                }
                if (_Customer == null)
                {
                    _Customer = new Customer()
                    {
                        CPNJ = splitValueLine[1].Trim(),
                        NameBusiness = splitValueLine[2].Replace('+', ' '),
                        Area = splitValueLine[3]
                    };
                }
                else
                {
                    lstCustomer.Remove(_Customer);
                    _Customer.NameBusiness = splitValueLine[2].Replace('+', ' ');
                    _Customer.Area = splitValueLine[3];
                }

                lstCustomer.Add(_Customer);
            }
        }

        private void AddSalesman(string[] splitValueLine)
        {
            if (splitValueLine.Length == 4)
            {
                Salesman SalesMan = null;
                if (lstSalesMan.Count > 0)
                {
                    SalesMan = (from p in lstSalesMan
                                where p.CPF.Trim() == splitValueLine[1].Trim()
                                select p).FirstOrDefault();
                }

                if (SalesMan == null)
                {
                    SalesMan = new Salesman()
                    {
                        CPF = splitValueLine[1].Trim(),
                        Name = splitValueLine[2].Replace('+', ' '),
                        Salary = (splitValueLine[3].IndexOf('.') > 0 ? Convert.ToDecimal(splitValueLine[3].Replace('+', ' ').Trim().Replace('.', ',')) : Convert.ToDecimal(splitValueLine[3].Replace('+', ' ').Trim()))
                    };
                }
                else
                {
                    lstSalesMan.Remove(SalesMan);
                    SalesMan.Name = splitValueLine[2].Replace('+', ' ');
                    SalesMan.Salary = (splitValueLine[3].IndexOf('.') > 0 ? Convert.ToDecimal(splitValueLine[3].Replace('+', ' ').Trim().Replace('.', ',')) : Convert.ToDecimal(splitValueLine[3].Replace('+', ' ').Trim()));
                }

                lstSalesMan.Add(SalesMan);
            }
        }

        private void CopyFileDir(string sourceFile, string destFile)
        {
            if (System.IO.File.Exists(sourceFile))
                try
                {
                    if (System.IO.File.Exists(sourceFile))
                    {
                        // Para copiar um arquivo para outro local e
                        // Sobrescrever o arquivo de destino, se já existir.
                        if (!System.IO.File.Exists(destFile))
                            System.IO.File.Copy(sourceFile, destFile, true);

                    }
                }
                catch (System.IO.IOException e)
                {
                    Console.WriteLine(e.Message);
                }
        }

        private void RemoveFileDir(string sourceFile)
        {
            // Excluir um arquivo usando o método estático de classe de arquivo
            if (System.IO.File.Exists(sourceFile))
            {
                try
                {
                    System.IO.File.Delete(sourceFile);
                }
                catch (System.IO.IOException e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        private void AnalyzeData(string sourceFile)
        {
            if (System.IO.File.Exists(sourceFile) && (this.lstCustomer.Count > 0 || this.lstSalesMan.Count > 0 || this.lstSales.Count > 0))
            {
                string fileName = "exportData";
                string nomeArquivo = @"C:\ImportData\Data\out\" + fileName + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".dat";

                // Cria um novo arquivo e devolve um StreamWriter para ele

                StreamWriter writer = new StreamWriter(nomeArquivo);

                // Agora é só sair escrevendo
                int QtsClientes = this.lstCustomer.Count;
                int QtsVendedor = this.lstSalesMan.Count;
                decimal _maxPrice = (this.lstSales.Count > 0 ? this.lstSales.Max(c => c.TotalPrice) : 0);
                decimal _minPrice = (this.lstSales.Count > 0 ? this.lstSales.Min(c => c.TotalPrice) : 0);
                int IdVendaMaisCara = (from p in this.lstSales
                                       where p.TotalPrice == _maxPrice
                                       select p.Sale_Id).FirstOrDefault();
                string WorstSeller = (from p in this.lstSales
                                      where p.TotalPrice == _minPrice
                                      select p.SalesMan_Name).FirstOrDefault();

                writer.WriteLine("QtsClientes-QtsVendedor-IdVendaMaisCara-PiorVendedor");
                writer.WriteLine(QtsClientes + "-" + QtsVendedor + "-" + IdVendaMaisCara + "-" + WorstSeller);

                writer.Close();
            }
        }

        #endregion

    }
}
