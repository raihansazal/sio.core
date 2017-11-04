﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Swastka.Cms.Api.Controllers;
using Swastika.Cms.Lib.Models;
using Swastika.Domain.Core.Models;
using Swastika.Cms.Lib.ViewModels;
using Swastika.Common.Helper;
using Microsoft.Data.OData.Query;
using System.Linq.Expressions;
using System;

namespace Swastka.IO.Cms.Api.Controllers
{
    [Produces("application/json")]
    [Route("api/{culture}/Modules")]
    public class ModulesController :
        BaseAPIController<SiocCmsContext, SiocArticle>
    {
        // GET api/articles/id
        [HttpGet]
        [Route("full/{id}")]
        public async Task<RepositoryResponse<ModuleWithDataViewModel>> Details(int id)
        {
            return await ModuleWithDataViewModel.Repository.GetSingleModelAsync(model => model.Id == id && model.Specificulture == _lang);
        }

        // GET api/articles/id
        [HttpPost]
        [Route("addToArticle")]
        public async Task<RepositoryResponse<bool>> AddToArticle([FromBody]ArticleModuleFEViewModel view)
        {
            if (view.IsActived)
            {
                var addResult = await view.SaveModelAsync();
                return new RepositoryResponse<bool>()
                {
                    IsSucceed = addResult.IsSucceed,
                    Data = addResult.IsSucceed
                };
            }
            else
            {
                return await view.RemoveModelAsync();
            }
            
        }

        // GET api/modules
        [HttpGet]
        [Route("")]
        [Route("{pageSize:int?}/{pageIndex:int?}")]
        [Route("{orderBy}/{direction}")]
        [Route("{pageSize:int?}/{pageIndex:int?}/{orderBy}/{direction}")]
        public async Task<RepositoryResponse<PaginationModel<ModuleListItemViewModel>>> Get(
            int? pageSize = 15, int? pageIndex = 0, string orderBy = "Id"
            , OrderByDirection direction = OrderByDirection.Ascending)
        {
            var data = await ModuleListItemViewModel.Repository.GetModelListByAsync(m => m.Specificulture == _lang, orderBy, direction, pageSize, pageIndex); //base.Get(orderBy, direction, pageSize, pageIndex);
            string domain = string.Format("{0}://{1}", Request.Scheme, Request.Host);
            data.Data.Items.ForEach(d => d.DetailsUrl = string.Format("{0}{1}", domain, this.Url.Action("Details", "modules", new { id = d.Id })));
            data.Data.Items.ForEach(d => d.EditUrl = string.Format("{0}{1}", domain, this.Url.Action("Edit", "modules", new { id = d.Id })));
            return data;
        }

        // GET api/modules
        [HttpGet]
        [Route("{keyword}")]
        [Route("{pageSize:int?}/{pageIndex:int?}/{keyword}")]
        [Route("{pageSize:int?}/{pageIndex:int?}/{orderBy}/{direction}/{keyword}")]
        public async Task<RepositoryResponse<PaginationModel<ModuleListItemViewModel>>> Search(
            string keyword = null, int? pageSize = null, int? pageIndex = null, string orderBy = "Id"
            , OrderByDirection direction = OrderByDirection.Ascending)
        {
            Expression<Func<SiocModule, bool>> predicate = model =>
            model.Specificulture == _lang &&
            (
            string.IsNullOrWhiteSpace(keyword)
                || (model.Title.Contains(keyword) || model.Description.Contains(keyword))
                );
            return await ModuleListItemViewModel.Repository.GetModelListByAsync(predicate, orderBy, direction, pageSize, pageIndex); // base.Search(predicate, orderBy, direction, pageSize, pageIndex, keyword);
        }
    }
}