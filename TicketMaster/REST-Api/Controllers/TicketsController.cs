﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using REST_Api.ApiModels;

namespace REST_Api.Controllers
{
    [Route("api/tickets")]
    [ApiController]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketRepo _repo;

        public TicketsController(ITicketRepo repo)
        {
            _repo = repo;
        }

        // GET: api/tickets
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Tickets>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAsync([FromQuery]string search = null)
        {
            IEnumerable<Domain.Models.Tickets> tickets = await _repo.GetTicketsAsync(search);
            if (tickets.Count() == 0)
            {
                return NotFound("Search returned no results.");
            }
            else
            {
                IEnumerable<Tickets> resource = tickets.Select(Mapper.MapTickets);
                return Ok(resource);
            }
        }

        // GET: api/tickets/string
        //[HttpGet("{since}")]
        //[ProducesResponseType(typeof(IEnumerable<Tickets>), StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //public async Task<IActionResult> GetByDatetimeAsync([FromQuery]DateTime since)
        //{
        //    IEnumerable<Domain.Models.Tickets> tickets = await _repo.GetTicketsAsync(since);

        //    IEnumerable<Tickets> resource = tickets.Select(Mapper.MapTickets);
        //    return Ok(resource);
        //}


        // GET: api/tickets/string,int
        [HttpGet("{searchType},{id}")]
        [ProducesResponseType(typeof(Tickets), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByIdAsync(string searchType, int id)
        {

            if (searchType.ToLower() == "admin")
            {
                if (await _repo.GetAdminByIdAsync(id) is Domain.Models.Admins admin)
                {
                    if (await _repo.GetTicketsByAdminAsync(id) is IEnumerable<Domain.Models.Tickets> tickets)
                    {
                        if (tickets.Count() == 0)
                        {
                            return NotFound("Admin has no tickets");
                        }
                        else
                        {
                            IEnumerable<Tickets> resource = tickets.Select(Mapper.MapTickets);
                            return Ok(resource);
                        }
                    }
                    return NotFound("dunno wha happen");
                }
                return BadRequest("Incorrect Id. Admin does not exist.");
            }

            else if (searchType.ToLower() == "store")
            {
                if (await _repo.GetStoreByIdAsync(id) is Domain.Models.Stores store)
                {
                    if (await _repo.GetTicketsByStoreAsync(id) is IEnumerable<Domain.Models.Tickets> tickets)
                    {
                        if (tickets.Count() == 0)
                        {
                            return NotFound("Store has no tickets");
                        }
                        else
                        {
                            IEnumerable<Tickets> resource = tickets.Select(Mapper.MapTickets);
                            return Ok(resource);
                        }
                    }
                    return NotFound("dunno wha happen");
                }
                return BadRequest("Incorrect Id. Store does not exist.");
            }

            else if (searchType.ToLower() == "user")
            {
                if (await _repo.GetUserByIdAsync(id) is Domain.Models.Users user)
                {
                    if (await _repo.GetTicketsByUserAsync(id) is IEnumerable<Domain.Models.Tickets> tickets)
                    {
                        if (tickets.Count() == 0)
                        {
                            return NotFound("User has no tickets");
                        }
                        else
                        {
                            IEnumerable<Tickets> resource = tickets.Select(Mapper.MapTickets);
                            return Ok(resource);
                        }
                    }
                    return NotFound("dunno wha happen");
                }
                return BadRequest("Incorrect Id. User does not exist.");
            }

            else if (searchType.ToLower() == "ticket")
            {
               
                    if (await _repo.GetTicketByIdAsync(id) is Domain.Models.Tickets ticket)
                    {
                       
                            Tickets resource = Mapper.MapTickets(ticket);
                            return Ok(resource);
                        
                    }
                    return NotFound("Incorrect ticket id. Ticket does not exist.");
                
            }

            //else if (searchType.ToLower() == "complete")
            //{
            //    IEnumerable<Domain.Models.Tickets> tickets = await _repo.GetTicketsByUserAsync(id);
            //            if (tickets.Count() == 0)
            //            {
            //                return NotFound("User has no tickets");
            //            }
            //            else
            //            {
            //                IEnumerable<Tickets> resource = tickets.Select(Mapper.MapTickets);
            //                return Ok(resource);
            //            }

            //        return NotFound("dunno wha happen");

            //    return BadRequest("Incorrect Id. User does not exist.");
            //}

            else
            {
                return BadRequest("Incorrect search type. Please enter 'admin', 'store', 'ticket', or 'user'.");
            }
        }


        // POST: api/tickets
        [HttpPost]
        [ProducesResponseType(typeof(Tickets), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PostAsync([FromBody]Tickets newTicket)
        {
            var ticket = Mapper.MapTickets(newTicket);
            _repo.AddTicketAsync(ticket);
            await _repo.SaveAsync();
            if (await _repo.GetLatestTicketAsync() is Domain.Models.Tickets newEntity)
            {
                return Ok(newEntity);
            }
            else //if (await _repo.GetLatestTicketAsync() is null)
            {
                return StatusCode(500, "Ticket is improperly formatted");
            }
        }

        // PUT: api/tickets/5
        [HttpPut]
        [ProducesResponseType(typeof(Tickets), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PutAsync([FromBody]Tickets ticket)
        {
            if (await _repo.GetTicketByIdAsync(ticket.Id) is Domain.Models.Tickets t)
            {
                var resource = Mapper.MapTickets(ticket);
                _repo.UpdateTicketAsync(ticket.Id, resource);
                await _repo.SaveAsync();
                var newEntity = await _repo.GetTicketByIdAsync(ticket.Id);
                return Ok(newEntity);
            }
            return NotFound("Ticket id doesn't exist");
        }

        // PUT: api/tickets/5
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(Tickets), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PutToCloseTicketAsync(int id)
        {
            if (await _repo.GetTicketByIdAsync(id) is Domain.Models.Tickets t)
            {
                
                _repo.CloseTicketAsync(id);
                await _repo.SaveAsync();
                var newEntity = await _repo.GetTicketByIdAsync(id);
                return Ok(newEntity);
            }
            return NotFound("Ticket id doesn't exist");
        }


        // PUT: api/tickets/5
        [HttpPut("{ticketId},{adminId}")]
        [ProducesResponseType(typeof(Tickets), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PutToReassignTicketAsync(int ticketId, int adminId)
        {
            if (await _repo.GetTicketByIdAsync(ticketId) is Domain.Models.Tickets 
                && await _repo.GetAdminByIdAsync(adminId) is Domain.Models.Admins )
            {

               var newEntity = await _repo.ReassignTicketAsync(ticketId, adminId);
                await _repo.SaveAsync();
                return Ok(newEntity);
            }
            else if (await _repo.GetTicketByIdAsync(ticketId) is null)
            {
                return NotFound("Ticket id does not exist");
            }
            else
                return NotFound("Admin id does not exist");
        }


        // DELETE: api/tickets/5
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(Admins), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            if (await _repo.GetTicketByIdAsync(id) is Domain.Models.Tickets t)
            {
                _repo.DeleteTicketAsync(id);
                await _repo.SaveAsync();
                return Ok("Ticket removed.");
            }
            return NotFound("Ticket doesn't exist");
        }
    }
}