using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.Models;
using API.Models.ToDoList;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ToDoListController : ControllerBase
    {
        private TempleDbContext _context;
        public ToDoListController(TempleDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        public async Task<ResultModel> Get()
        {
            ResultModel result = new ResultModel();
            List<ToDoListItem> ToDoList = new List<ToDoListItem>();
            var query = new List<ToDoListItem>();
            try
            {
                query = _context.ToDoLists.ToList();
            }
            catch (Exception e)
            {
                result.IsSuccess = false;
                result.Message = e.InnerException.Message;
                return result;
            }
            result.IsSuccess = true;
            result.Data = query;
            return result;
        }


        ////// <summary>
        ///新增代辦項目
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> Post(ToDoListItem model)
        {
            var result = new ResultModel();
            model.CreatedDate = DateTime.Now;
            _context.ToDoLists.Add(model);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                result.IsSuccess = false;
                result.Message = e.Message;
                return result;
            }

            result.IsSuccess = true;
            result.Data = new { model.Id, model.Subject};
            result.Message = "新增成功";
            return result;


        }

        ////// <summary>
        ///編輯代辦項目
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        public async Task<ResultModel> Edit(ToDoListItemForEdit model)
        {
            var result = new ResultModel();
            var existData = _context.ToDoLists.Find(model.Id);
            if (existData == null)
            {
                result.IsSuccess = false;
                result.Message = "Data is not found";
                return result;
            }


            foreach (var property1 in model.GetType().GetProperties())
            {
                foreach (var property2 in existData.GetType().GetProperties())
                {

                    if (property1.Name == property2.Name)
                    {
                        if (property1.GetValue(model) != null)
                        {
                            property2.SetValue(existData, property1.GetValue(model));
                        }
                    }
                }

            }
            existData.EditDate = DateTime.Now;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                result.IsSuccess = false;
                result.Message = e.Message;
                return result;
            }

            result.IsSuccess = true;
            result.Message = "編輯成功";
            return result;


        }

        [HttpDelete("{Id}")]
        public async Task<ResultModel> Delete(int Id)
        {
            var result = new ResultModel();
            var existData = _context.ToDoLists.Find(Id);
            if (existData == null)
            {
                result.IsSuccess = false;
                result.Message = "Data is not exist.";
                return result;
            }
            _context.ToDoLists.Remove(existData);
            try
            {
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                result.IsSuccess = false;
                result.Message = e.InnerException.Message;
                return result;
            }

            result.IsSuccess = true;
            result.Message = "刪除成功";
            return result;
        }
    }
}