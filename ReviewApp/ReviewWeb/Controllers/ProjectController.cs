﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using ReviewWeb.Models.ProjectModels;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.Text;
using Microsoft.AspNetCore.Mvc.Rendering;
using ReviewWeb.Models.Artifact;
using Microsoft.Extensions.Configuration;

namespace ReviewWeb.Controllers
{
    [Route("Project")]
    public class ProjectController : Controller
    {
        readonly string siteName;
        public ProjectController(IConfiguration configuration)
        {
            this.siteName = configuration.GetValue<string>("Websetting:Url");
        }
        public IActionResult ShowProject(int id)
        {
            ProjectViewModel project = new ProjectViewModel();
            ViewBag.Project = project;
            return View("Projects");
        }
        [Route("ShowProjects")]
        public async Task<IActionResult> ShowProjects()
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("SecurityToken"));
                HttpResponseMessage message = await client.GetAsync(siteName+"/api/Project/GetAllProjects");
                if (message.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    RedirectToAction("LoginPage", "Login");
                else
                {
                    string s = await message.Content.ReadAsStringAsync();
                    var projects = JsonConvert.DeserializeObject<List<ProjectViewModel>>(s);
                    ViewBag.Projects = projects;
                }
            }
            return View("Project");
        }
        [Route("CreateProject")]
        public async Task<IActionResult> CreateProject()
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("SecurityToken"));
                HttpResponseMessage message = await client.GetAsync(siteName + "/api/Project/GetAllProjectTypes");
                var projectTypes = JsonConvert.DeserializeObject<List<ProjectTypeViewModel>>(await message.Content.ReadAsStringAsync());
                List<SelectListItem> projectTypesSelect = new List<SelectListItem>();
                foreach(var pt in projectTypes)
                {
                    projectTypesSelect.Add(new SelectListItem() { Text = pt.Name, Value = pt.Id.ToString() });
                }
                ViewBag.projectTypes = projectTypesSelect;

                return View("CreateProject");
            }
                
        }
        [Route("SaveProject")]
        [HttpPost]
        public async Task<IActionResult> SaveProject(CreateProjectViewModel model)
        {
            using (HttpClient client = new HttpClient())
            {
                string json = JsonConvert.SerializeObject(model);
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("SecurityToken"));
                HttpResponseMessage message = await client.PostAsync(siteName +"/api/Project/SaveProject", new StringContent(json, Encoding.UTF8, "application/json"));
            }
            return RedirectToAction("ShowProjects", "Project");
        }
        [Route("ProjectDetail")]
        [HttpGet]
        public async Task<IActionResult> ProjectDetail(int projectId)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("SecurityToken"));
                HttpResponseMessage message = await client.GetAsync(siteName + "/api/Project/ProjectDetail?id=" + projectId);
                var project = JsonConvert.DeserializeObject<ProjectDetailViewModel>(await message.Content.ReadAsStringAsync());
                ViewBag.Project = project;

                HttpResponseMessage m = await client.GetAsync(siteName + "/api/Review/GetReviewsForProject?id=" + projectId);
                var reviews = JsonConvert.DeserializeObject<List<ProjectReview>>(await m.Content.ReadAsStringAsync());

                ViewBag.Reviews = reviews;

                HttpResponseMessage tskmsg = await client.GetAsync(siteName+"/api/Project/GetTasks?projectId=" + projectId);
                List<TaskModel> tasks = JsonConvert.DeserializeObject<List<TaskModel>>(await tskmsg.Content.ReadAsStringAsync());
                ViewBag.Tasks = tasks;

            }
            return View("ProjectDetail");
        }
        [Route("SaveWorkProduct")]
        [HttpPost]
        public async Task<IActionResult> SaveWorkProduct(WorkProductViewModel workProduct)
        {
            using (HttpClient client = new HttpClient())
            {
                string json = JsonConvert.SerializeObject(workProduct);
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("SecurityToken"));
                HttpResponseMessage message = await client.PostAsync(siteName + "/api/Project/SaveWorkProduct", new StringContent(json, Encoding.UTF8, "application/json"));

            }
            return RedirectToAction("ProjectDetail", "Project", new { projectId = workProduct.ProjectId });
        }
        [Route("GetWorkProductDetail")]
        [HttpGet]
        public async Task<IActionResult>GetWorkProductDetail(int id)
        {
            using(HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("SecurityToken"));
                HttpResponseMessage message = await client.GetAsync(siteName + "/api/Project/GetWorkProductDetail?id=" + id);
                var workProductDetail = JsonConvert.DeserializeObject<WorkProductViewModel>(await message.Content.ReadAsStringAsync());
                ViewBag.workProductDetail = workProductDetail;
                HttpResponseMessage msg = await client.GetAsync(siteName + "/api/Artifact/GetAllArtifactForWorkProduct?id=" + id);
                HttpResponseMessage msgReviews = await client.GetAsync(siteName + "/api/Review/GetReviewsForWorkProduct?id=" + id);
                var reviews = JsonConvert.DeserializeObject<List<ProjectReview>>(await msgReviews.Content.ReadAsStringAsync());
                ViewBag.Reviews = reviews;
                
                var artifacts = JsonConvert.DeserializeObject<List<JazzArtifact>>(await msg.Content.ReadAsStringAsync());
                ViewBag.Artifacts = artifacts;
               
                return View("WorkProductDetail");
            }
            
        }
        [HttpPut("ChangeWorkProduct")]
        public async Task<IActionResult>ChangeWorkProduct([FromBody] WorkProductViewModel model)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("SecurityToken"));
                string json = JsonConvert.SerializeObject(model);
                HttpResponseMessage message = await client.PutAsync(siteName + "/api/Project/ChangeWorkProduct", new StringContent(json, Encoding.UTF8, "application/json"));
                return Ok();
            }
        }
        [Route("DeleteProject")]
        public async Task<IActionResult>DeleteProject(int id)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("SecurityToken"));
                HttpResponseMessage message = await client.DeleteAsync(siteName + "/api/Project/DeleteProject?id=" + id);
                return RedirectToAction("ShowProjects", "Project");
            }
        }
        [Route("DeleteWorkproduct")]
        public async Task<IActionResult>DeleteWorkProduct(int id, int projectid)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("SecurityToken"));
                HttpResponseMessage message = await client.DeleteAsync(siteName +"/api/Project/DeleteWorkProduct?id=" + id);
                return RedirectToAction("ProjectDetail", "Project", new { projectId = projectid });
            }
        }
        [HttpPost]
        public async Task<IActionResult>GetTasksFromIBM(IbmUrlViewModel model)
        {
            using (HttpClient client = new HttpClient())
            {
                string json = JsonConvert.SerializeObject(model);
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("SecurityToken"));
                HttpResponseMessage message = await client.PostAsync(siteName + "/api/Project/GetTasksFromIBM", new StringContent(json, Encoding.UTF8, "application/json"));
                return RedirectToAction("ProjectDetail", "Project", new { projectId = model.ProjectId });
            }
        }
        [Route("TaskPlanning")]
        public IActionResult TaskPlanning(int id)
        {
            ViewBag.ProjectId = id;
            return View("~/Views/Project/PlanedTask.cshtml");
        }

    }
}