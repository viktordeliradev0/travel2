﻿using Microsoft.AspNetCore.Mvc;
using travelingExperience.Models;
using Scrypt;
using travelingExperience.DbConnetion;
using Microsoft.AspNetCore.Identity;
using travelingExperience.Data;
using travelingExperience.Data.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace travelingExperience.Controllers
{
    public class UserController : Controller
    {
        private readonly AppDbContext _db;
        private readonly ScryptEncoder encoder;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _singInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ITravelsService _travelsService;
        private readonly CommentService _commentService;
        private readonly ReservationService _reservationService;


        public UserController(AppDbContext db, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> singInManager, RoleManager<IdentityRole> roleManager, ITravelsService travelsService, CommentService commentService, ReservationService reservationService)
        {
            encoder = new ScryptEncoder();
            _singInManager = singInManager;
            _roleManager = roleManager;
            _db = db;
            _userManager = userManager;
            _travelsService = travelsService;
            _commentService = commentService;
            _reservationService = reservationService;
        }
        [HttpGet]
        public async Task<IActionResult> UserTravels()
        {
            var userId = _userManager.GetUserId(User);
            var userTravels = await _travelsService.GetTravelsByUserIdAsync(userId);

            return View(userTravels);
        }
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);


            if (user == null)
            {
                return NotFound(); 
            }
            var userId = user.Id;
            var comments = _commentService.GetCommentsByUserId(userId);

            var model = new UserProfileViewModel
            {
                Name = user.Name,
                SName = user.SName,
                UserName = user.UserName,
                Email = user.Email,
                Number = user.Number,
                Age = user.Age,
                Comments = user.Comments,
            };
           
            return View(model);


        }
        private async Task<ApplicationUser> GetUserByIdAsync(string userId)
        {
            return await _db.Users.FindAsync(userId);
        }

        private async Task<ApplicationUser> GetUserByIdOrNotFoundAsync(string userId)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null)
            {
                return null; // or handle the case where user is not found
            }

            return user;
        }
        public async Task<IActionResult> ProfileView(string id)
        {
            var user = await GetUserByIdOrNotFoundAsync(id);
            
            if (user == null)
            {
                return NotFound();
            }

           var comments = _commentService.GetCommentsByUserId(id);

            var model = new UserProfileViewModel
            {
                Id = id,
                Name = user.Name,
                SName = user.SName,
                UserName = user.UserName,
                Email = user.Email,
                Number = user.Number,
                Age = user.Age,
                Comments = user.Comments
            };

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(string userId, string commentText)
        {
          
            var user = await GetUserByIdAsync(userId);


            if (user == null)
            {
                return NotFound(); 
            }

            var newComment = new Comment
            {
                UserID = userId,
                CommentText = commentText,
                CommentDate = DateTime.Now
            };

            user.Comments = user.Comments ?? new List<Comment>();
            user.Comments.Add(newComment);

            await _commentService.SaveChangesAsync();

            return RedirectToAction("ProfileView", new { id = userId });
        }

        public IActionResult Index()
        {
            var users = _userManager.Users.ToList();
            return View(users);
        }
        public IActionResult Login()
        {
            return View();
        }
        public async Task<IActionResult> Register()
        {
            if (!_roleManager.RoleExistsAsync(Helper.Admin).GetAwaiter().GetResult())
            {
                await _roleManager.RoleExistsAsync(Helper.Admin);
                await _roleManager.RoleExistsAsync(Helper.User);
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _singInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", "Invalid Login attempt!");
            }
          
            return View(model);
        }
      


        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Register(RegisterModel model)
        {



            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    Name = model.Name,
                    SName = model.SName,
                    Number = model.Number,
                    Age = model.Age
                };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, model.RoleName);
                    await _singInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }   

        [HttpPost]
        public async Task<IActionResult> Logoff()
        {
            await _singInManager.SignOutAsync();
            return RedirectToAction("Login", "User");
        }

       
        public async Task<IActionResult> MyReservations()
        {
            // Retrieve the current user
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Index");
            }
            var userId = user.Id;

            List<Reserve> reservations = _reservationService.GetReservationsForUser(userId);


            return View(reservations);
        }
        [HttpPost]
        public async Task<IActionResult> CancelReservation(int reservationId)
        {
            // Retrieve the reservation
            var reservation = await _db.Reserves.FindAsync(reservationId);

            // Check if the reservation exists
            if (reservation == null)
            {
                return NotFound(); // Handle the case where the reservation is not found
            }

            // Retrieve the associated travel
            var travel = await _db.Travels.FindAsync(reservation.TravelID);

            // Update the available seats in the travel
            travel.Seats += reservation.ReservedSeats;

            // Remove the reservation from the context and save changes
            _db.Reserves.Remove(reservation);
            await _db.SaveChangesAsync();

            return RedirectToAction("MyReservations");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> DeleteUser(string userId)
        {
            try
            {
                // Ensure userId is not null or empty
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest("User ID is required.");
                }

                // Get the user by Id
                var user = await GetUserByIdAsync(userId);

                // Check if the user exists
                if (user == null)
                {
                    return NotFound(); // Or handle the case where user is not found
                }

                // Remove the user from the database
                _db.Users.Remove(user);
                await _db.SaveChangesAsync();

                // Redirect to appropriate action after deletion
                return RedirectToAction("Index"); // Or any other action as needed
            }
            catch (Exception ex)
            {
                // Log the error or display an error message
                ModelState.AddModelError("", "Failed to delete the user. Please try again later.");
                return View("Error");
            }
        }





        [HttpPost]
        public IActionResult SaveTravel(int travelId)
        {
            // Get the current user
            var user = _userManager.GetUserAsync(User).Result;
            if (user == null)
            {
                // Handle case when user is not authenticated
                return RedirectToAction("Login", "Account");
            }

            // Check if the travel with the specified ID exists
            var travel = _db.Travels.FirstOrDefault(t => t.Id == travelId);
            if (travel == null)
            {
                // Handle case when the specified travel does not exist
                return NotFound();
            }

            // Check if the travel is already saved by the user
            var savedTravel = _db.UserSavedTravel
                .FirstOrDefault(st => st.TravelId == travelId && st.UserId == user.Id);
            if (savedTravel != null)
            {
                // Handle case when the travel is already saved by the user
                return RedirectToAction("Index1", "Travel");
            }

                // Save the travel for the user
                _db.UserSavedTravel.Add(new UserSavedTravel { UserId = user.Id, TravelId = travelId });
            _db.SaveChanges();

            return RedirectToAction("Index1", "Travel");
        }





        public IActionResult MySavedTravels()
        {
            // Get the current user
            var user = _userManager.GetUserAsync(User).Result;
            if (user == null)
            {
                // Handle case when user is not authenticated
                return RedirectToAction("Login", "Account");
            }

            // Get the saved travels for the current user
            var savedTravels = _db.UserSavedTravel
                .Where(st => st.UserId == user.Id)
                .Select(st => st.Travel)
                .ToList();

            return View(savedTravels);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnsaveTravel(int travelId)
        {
            // Retrieve the current user's ID
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            // Find the saved travel for the current user
            var savedTravel = await _db.UserSavedTravel.SingleOrDefaultAsync(s => s.TravelId == travelId && s.UserId == userId);

            if (savedTravel == null)
            {
                return NotFound();
            }

            // Remove the saved travel
            _db.UserSavedTravel.Remove(savedTravel);
            await _db.SaveChangesAsync();

            // Redirect to a page or action method
            return RedirectToAction("Index", "Home"); // For example
        }
    }


}


