﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ReviewWeb.Models.Artifact;
using System.Net.Http;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace ReviewWeb.Controllers
{
    [Route("Artifact")]
    public class ArtifactController : Controller
    {
        readonly string siteName;
        public ArtifactController(IConfiguration configuration)
        {
            this.siteName = configuration.GetValue<string>("Websetting:Url");
        }
        [HttpGet("SpecificArtifacts")]
        public IActionResult SpecificArtifacts(int projectId, int workProductId)
        {
            ViewBag.projectId = projectId;
            ViewBag.workProductId = workProductId;
            return View("~/Views/Review/ArtifactsSpecification.cshtml");
        }
        //[HttpPost("CheckIbmXml")]
        [HttpPost]
        public async Task<IActionResult> CheckIbmXml(/*[FromBody]*/ IbmUrlViewModel model)
        {
            
            using (HttpClient client = new HttpClient())
            {
                string json = JsonConvert.SerializeObject(model);
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("SecurityToken"));
                HttpResponseMessage message = await client.PostAsync(siteName + "/api/Artifact/GetArtifactsFormUrl", new StringContent(json, Encoding.UTF8, "application/json"));
                
            }
            return RedirectToAction("GetWorkProductDetail", "Project", new { id = model.WorkProductId });
        }
        [HttpGet]
        public async Task<IActionResult> ShowWorkProductArtifacts(int workProductId, int page)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage msg = await client.GetAsync(siteName + "/api/Artifact/GetArtifactsPerPage?workProductId=" + workProductId + "&&page=" + page);
                var artifacts = JsonConvert.DeserializeObject<List<JazzArtifact>>(await msg.Content.ReadAsStringAsync());
                HttpResponseMessage msg2 = await client.GetAsync(siteName + "/api/Artifact/NumberOfArtifactsInWorkProduct?workProductId=" + workProductId);
                int numberOfArtifact = JsonConvert.DeserializeObject<int>(await msg2.Content.ReadAsStringAsync());
                ViewBag.NumberOfPage = numberOfArtifact / 15;
                ViewBag.Artifacts = artifacts;
                ViewBag.workProduct = workProductId;
                ViewBag.ActivePage = page;
                ViewBag.WorkProductId = workProductId;
                return PartialView("~/Views/Shared/ArtifactsPagination.cshtml");
            }
        }
        [HttpGet("GetArtifctsHeadersForProject")]
        public async Task<List<string>> GetArtifctsHeadersForProject(int projectId)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("SecurityToken"));
                HttpResponseMessage msg2 = await client.GetAsync(siteName + "/api/Artifact/GetArtifactDetailsHeadersForProject?projectId=" + projectId);
                var headers = JsonConvert.DeserializeObject<List<string>>(await msg2.Content.ReadAsStringAsync());
                return headers;
            }
        }
        [HttpGet("GetAllArtifactForProject")]
        public async Task<List<JazzArtifact>> GetAllArtifactForProject(int projectId)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("SecurityToken"));
                HttpResponseMessage msg2 = await client.GetAsync(siteName + "/api/Artifact/GetAllArtifactForWorkProduct?id=" + projectId);
                var headers = JsonConvert.DeserializeObject<List<JazzArtifact>>(await msg2.Content.ReadAsStringAsync());
                return headers;
            }
        }


    }
}