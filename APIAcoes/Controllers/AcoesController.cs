using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using APIAcoes.Models;
using APIAcoes.Data;

namespace APIAcoes.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AcoesController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AcoesController> _logger;
        private readonly AcoesRepository _repository;

        public AcoesController(IConfiguration configuration,
            ILogger<AcoesController> logger,
            AcoesRepository repository)
        {
            _configuration = configuration;
            _logger = logger;
            _repository = repository;
        }

        [HttpGet("UltimoValor/{codigo}")]
        [ProducesResponseType(typeof(UltimaCotacaoAcao), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public ActionResult<UltimaCotacaoAcao> GetCotacao(string codigo)
        {
            if (String.IsNullOrWhiteSpace(codigo))
            {
                _logger.LogError(
                    $"GetCotacao - Codigo de Acao nao informado");
                return new BadRequestObjectResult(new
                {
                    Sucesso = false,
                    Mensagem = "Código de Ação não informado"
                });
            }

            _logger.LogInformation($"GetCotacao - codigo da Acao: {codigo}");
            UltimaCotacaoAcao acao = null;
            if (!String.IsNullOrWhiteSpace(codigo))
                acao = _repository.Get(codigo.ToUpper());

            if (acao != null)
            {
                _logger.LogInformation(
                    $"GetCotacao - Acao: {codigo} | Valor atual: {acao.Valor} | Ultima atualizacao: {acao.Data}");
                return new OkObjectResult(acao);
            }
            else
            {
                _logger.LogError(
                    $"GetCotacao - Codigo de Acao nao encontrado: {codigo}");
                return new NotFoundObjectResult(new
                {
                    Sucesso = false,
                    Mensagem = $"Código de Ação não encontrado: {codigo}"
                });
            }
        }

        [HttpGet]
        public ActionResult<List<HistoricoAcao>> GetAll()
        {
            var dados = _repository.GetAll().ToList();
            _logger.LogInformation($"GetAll - encontrado(s) {dados.Count} registro(s)");
            return dados;
        }
    }
}