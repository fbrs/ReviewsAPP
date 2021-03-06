﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ReviewApi.Models.Database;
using System.Security.Claims;
using ReviewApi.Models.Project;
using ReviewApi.Models.User;
using ReviewApi.Models.Artifact;
using Microsoft.EntityFrameworkCore;
using System.Net;
using ReviewApi.BusinessLogic;

namespace ReviewApi.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/Project")]
    public class ProjectController : Controller
    {
        ReviewsDatabaseContext context;
        public ProjectController(ReviewsDatabaseContext context)
        {
            this.context = context;
        }
        [Route("SaveProject")]
        [HttpPost]
        public IActionResult SaveProject([FromBody] ProjectModel project)
        {

            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                string email = identity.FindFirst("UserEmail").Value;
                Project p = new Project() { Name = project.Name, Description = project.Description, UsersEmail = email, ProjectTypeId = project.ProjectTypeId};
                context.Project.Add(p);
                context.SaveChanges();

                UserProject r = new UserProject() { ProjectId = p.Id, UsersEmail = email };
                context.UserProject.Add(r);
                context.SaveChanges();
                return Ok();
            }
            return Unauthorized();

        }
        [HttpGet]
        [Route("GetAllProjects")]
        public IEnumerable<Project> GetAllProjectsByParticipant()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                string email = identity.FindFirst("UserEmail").Value;
                
                var projectIDs = context.UserProject.Where(o => o.UsersEmail == email).Select(i => i.ProjectId).ToArray();
                
                var projects = context.Project.Where(p => projectIDs.Contains(p.Id)).ToArray();
                return projects;
            }
            return null;
        }
        [HttpGet]
        [Route("GetAllProjectTypes")]
        public IEnumerable<ProjectTypeModel>GetAllProjectTypes()
        {
            List<ProjectTypeModel> types = new List<ProjectTypeModel>();
            var projectTypes = context.ProjectType.ToArray();
            foreach(var p in projectTypes)
            {
                types.Add(new ProjectTypeModel() { Id = p.Id, Name = p.Name });
            }
            return types;
        }
        [HttpGet("id")]
        [Route("ProjectDetail")]
        public ProjectDetailModel GetProjectDetail(int id)
        {
            //throw new Exception();
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                //validate if user can access to specific projct
                string email = identity.FindFirst("UserEmail").Value;
                var hasUserProject = context.UserProject.Where(u => u.ProjectId == id && u.UsersEmail == email).FirstOrDefault();
                if (hasUserProject != null)
                    if (hasUserProject.UsersEmail == identity.FindFirst("UserEmail").Value)
                    {
                        ProjectDetailModel detailsModel = new ProjectDetailModel();
                        Project project = context.Project.Where(p => p.Id == id).FirstOrDefault();
                        ProjectModel model = new ProjectModel() { Id = project.Id, Description = project.Description, Name = project.Name, Owner = project.UsersEmail };
                        detailsModel.ProjectModel = model;
                        Workproduct[] workproducts = context.Workproduct.Where(p => p.ProjectId == id).ToArray();
                        List<WorkProductModel> wmodel = new List<WorkProductModel>();
                        foreach (Workproduct w in workproducts)
                        {
                            wmodel.Add(new WorkProductModel() { Name = w.Name, Description = w.Description, Id = w.Id, ProjectId = w.ProjectId });
                        }
                        detailsModel.WorkproductModel = wmodel.ToArray();
                        return detailsModel;
                    }
            }
            return null;
        }
        [HttpPost]
        [Route("SaveWorkProduct")]
        public ActionResult SaveWorkProduct([FromBody] Workproduct workproduct)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            workproduct.UsersEmail = identity.FindFirst("UserEmail").Value;
            Project p = context.Project.Where(o => o.Id == workproduct.ProjectId).FirstOrDefault();
            if (p == null)
                return NotFound(new { Message = "Project doesn't exist!" });
            if (workproduct.Name != "")
            {
                context.Workproduct.Add(workproduct);
                context.SaveChanges();
                return Ok();
            }

            return BadRequest(new { Message = "Name can't be empty" });
        }
        [HttpGet("id")]
        [Route("GetAllWorkProductsForProject")]
        public IEnumerable<WorkProductModel> GetAllWorkProductsForProject(int id)
        {
            List<WorkProductModel> workProducts = new List<WorkProductModel>();
            foreach (var w in context.Workproduct.Where(p => p.ProjectId == id))
            {
                workProducts.Add(new WorkProductModel() { Id = w.Id, Name = w.Name });
            }
            return workProducts;
        }
        [HttpGet("id")]
        [Route("GetAllParticipantsOnProject")]
        public IEnumerable<string> GetAllParticipantsOnProject(int id)
        {
            return context.UserProject.Where(p => p.ProjectId == id).Select(p => p.UsersEmail).ToArray();
        }

        [HttpPost]
        [Route("ChangeParticipants")]
        public ActionResult ChangeParticipantsOnProject([FromBody] Participant participants)
        {
            foreach (string s in participants.AddedUsers)
            {
                UserProject project = new UserProject() { ProjectId = participants.ProjectId, UsersEmail = s };
                context.UserProject.Add(project);
            }
            context.SaveChanges();
            List<UserProject> l = new List<UserProject>();
            foreach (string s in participants.RemovedUsers)
            {
                UserProject project = new UserProject() { ProjectId = participants.ProjectId, UsersEmail = s };
                l.Add(project);
            }
            context.UserProject.RemoveRange(l);
            context.SaveChanges();
            return Ok();
        }
        [HttpGet]
        [Route("GetWorkProductDetail")]
        public WorkProductModel GetWorkProductDetail(int id)
        {
            var workProduct = context.Workproduct.Where(w => w.Id == id).FirstOrDefault();
            WorkProductModel model = new WorkProductModel()
            {
                Id = workProduct.Id,
                Description = workProduct.Description,
                Name = workProduct.Name,
                ProjectId = workProduct.ProjectId
            };
            return model;

        }
        [HttpPut]
        [Route("ChangeWorkProduct")]
        public IActionResult ChangeWorkProduct([FromBody] WorkProductModel model)
        {
            var wk = context.Workproduct.Where(x => x.Id == model.Id).FirstOrDefault();
            wk.Name = model.Name;
            wk.Description = model.Description;
            context.SaveChanges();
            return Ok();
        }

        [HttpDelete]
        [Route("DeleteProject")]
        public IActionResult DeleteProject(int id)
        {
            var project = context.Project.Where(x => x.Id == id).FirstOrDefault();
            var workproducts = context.Workproduct.Where(x => x.ProjectId == project.Id).ToList();
            var userProject = context.UserProject.Where(x => x.ProjectId == project.Id).ToList();
            var reviews = context.Review.Where(x => workproducts.Select(p => p.Id).Contains(x.WorkproductId)).ToList();
            var reviewRoles = context.UserReviewRole.Where(x => reviews.Select(p => p.Id).Contains(x.ReviewId)).ToList();
            var artifacts = context.IbmArtifact.Where(x => workproducts.Select(p => p.Id).Contains(x.WorkproductId)).ToList();
            var headervalues = context.HeaderRowData.Where(x => reviews.Select(p => p.Id).Contains(x.ReviewId)).ToList();
            var ibmreview = context.IbmArtifactReview.Where(x => artifacts.Select(p => p.Id).Contains(x.IbmArtifactId)).ToList();

            context.IbmArtifactReview.RemoveRange(ibmreview);
            context.HeaderRowData.RemoveRange(headervalues);
            context.IbmArtifact.RemoveRange(artifacts);
            context.UserReviewRole.RemoveRange(reviewRoles);
            context.Review.RemoveRange(reviews);
            context.UserProject.RemoveRange(userProject);
            context.Workproduct.RemoveRange(workproducts);
            context.Project.Remove(project);
            context.SaveChanges();
            return Ok();
        }
        [HttpDelete]
        [Route("DeleteWorkProduct")]
        public IActionResult DeleteWorkProduct(int id)
        {
            var workproducts = context.Workproduct.Where(x => x.Id == id).FirstOrDefault();
            var reviews = context.Review.Where(x => x.WorkproductId == workproducts.Id).ToList();
            var reviewRoles = context.UserReviewRole.Where(x => reviews.Select(p => p.Id).Contains(x.ReviewId)).ToList();
            var artifacts = context.IbmArtifact.Where(x => x.WorkproductId == workproducts.Id).ToList();
            var headervalues = context.HeaderRowData.Where(x => reviews.Select(p => p.Id).Contains(x.ReviewId)).ToList();
            var ibmreview = context.IbmArtifactReview.Where(x => artifacts.Select(p => p.Id).Contains(x.IbmArtifactId)).ToList();
            context.IbmArtifactReview.RemoveRange(ibmreview);
            context.HeaderRowData.RemoveRange(headervalues);
            context.IbmArtifact.RemoveRange(artifacts);
            context.UserReviewRole.RemoveRange(reviewRoles);
            context.Review.RemoveRange(reviews);
            context.Workproduct.Remove(workproducts);
            context.SaveChanges();
            return Ok();
        }
        [HttpPost]
        [Route("GetTasksFromIBM")]
        public IActionResult GetTasksFromIBM([FromBody] IbmUrlModel model)
        {
            string xml;
            List<TaskModel> nodesInXml = null;
            using (WebClient client = new WebClient())
            {
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                String username = model.Name;
                String password = model.Password;
                String encoded = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + password));
                client.Headers.Add("Authorization", "Basic " + encoded);
                xml = client.DownloadString(model.Url);
                nodesInXml = XmlParser.CreateTaskObjects(xml);
                SaveToDatabase(nodesInXml, model.ProjectId);
                return Ok();
            }
        }
        private void SaveToDatabase(List<TaskModel> tasks, int projectId)
        {
            Project p = context.Project.Where(x => x.Id == projectId).FirstOrDefault();
            foreach(var t in tasks)
            {
                TaskPlan plan = new TaskPlan() { Name = t.Name, IbmId = t.IbmId, Type = t.Type, Url = t.Url };
                p.TaskPlan.Add(plan);
            }
            context.SaveChanges();
            
        }
        [HttpGet]
        [Route("GetTasks")]
        public List<TaskModel> GetTasks(int projectId)
        {
            var tasks = context.TaskPlan.Where(x => x.ProjectId == projectId).ToList();
            List<TaskModel> tasksForProject = new List<TaskModel>();
            foreach(var t in tasks)
            {
                TaskModel model = new TaskModel() { IbmId = t.IbmId, Id = t.Id, Name = t.Name, Type = t.Type, Url = t.Url };
                tasksForProject.Add(model);
            }
            return tasksForProject;
        }
    }
}