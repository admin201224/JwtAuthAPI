# Hý?ng D?n Chi Ti?t: Tính Nãng Progress Bar & T?m Ki?m Khóa H?c

## ?? PH?N 1: ÐÁNH D?U HOÀN THÀNH BÀI H?C + PROGRESS BAR

### Tính Nãng
- ? H?c viên có th? ðánh d?u "Ð? hoàn thành" cho t?ng bài h?c
- ? Thanh ti?n ð? (Progress Bar) hi?n th? % hoàn thành khóa h?c
- ? Checkbox và checkmark hi?n th? tr?ng thái hoàn thành
- ? D? li?u ðý?c lýu vào database

### Cách S? D?ng

#### Cho H?c Viên:
1. **Truy c?p trang h?c:** `MyCourses/Study/{courseId}`
2. **Xem ti?n ð?:**
   - Thanh progress bar ? sidebar trái hi?n th? % hoàn thành
   - Hi?n th? "X / Y bài hoàn thành"
3. **Ðánh d?u hoàn thành:**
   - M? m?t bài h?c
   - Tích vào checkbox "Ðánh d?u ð? hoàn thành"
   - Thanh progress t? c?p nh?t
   - Bi?u tý?ng ? màu xanh hi?n trên bài trong sidebar

#### API Endpoints (Backend):
```
GET  /api/lesson-progress/get-or-create/{contentId}/{courseId}
  ? L?y ho?c t?o ti?n ð? cho m?t bài

POST /api/lesson-progress/update/{contentId}
  Body: { "isCompleted": true, "progressPercentage": null }
  ? C?p nh?t tr?ng thái hoàn thành

GET  /api/lesson-progress/course/{courseId}
  ? L?y ti?n ð? toàn khóa h?c (%)

GET  /api/lesson-progress/course/{courseId}/lessons
  ? L?y ti?n ð? t?ng bài h?c
```

### Database Schema
**B?ng: LessonProgresses**
```
Id (PK)
UserId (FK) ? Users
ContentId (FK) ? CourseContents
CourseId (FK) ? Courses
IsCompleted (bool) - Ð? hoàn thành?
ProgressPercentage (int) - 0-100%
StartedAt (DateTime)
CompletedAt (DateTime)
```

### Các File T?o Ra:
1. `JwtAuthAPI/Models/LessonProgress.cs` - Entity model
2. `JwtAuthAPI/Models/LessonProgressDto.cs` - DTO classes
3. `JwtAuthAPI/Services/LessonProgressService.cs` - Business logic
4. `JwtAuthAPI/Controllers/LessonProgressController.cs` - API endpoints
5. `CourseManagementMVC/Models/ViewModels.cs` - ViewModels (ðý?c c?p nh?t)

### C?n Làm:
1. **T?o Migration** (d?ng app, ch?y l?nh):
   ```bash
   cd JwtAuthAPI
   dotnet ef migrations add AddLessonProgress --context ApplicationDbContext
   dotnet ef database update
   ```

2. **Frontend JavaScript** - Ð? tích h?p s?n trong `MyCourses/Study.cshtml`:
   - T? ð?ng load ti?n ð? khi vào trang
   - Auto-update progress bar
   - AJAX request khi checkbox thay ð?i

### Workflow:
```
User clicks checkbox
  ?
JavaScript: UpdateLessonProgress() ? AJAX POST
  ?
Controller: MyCoursesController/UpdateLessonProgress
  ?
ApiService: UpdateLessonProgressAsync()
  ?
Backend API: LessonProgressController/Update
  ?
Service: LessonProgressService.UpdateProgressAsync()
  ?
Database: UPDATE LessonProgresses SET IsCompleted=1, CompletedAt=NOW()
  ?
Response: JSON { success: true, data: {...} }
  ?
JavaScript: LoadCourseProgress() ? Update Progress Bar
```

---

## ?? PH?N 2: T?M KI?M VÀ L?C KHÓA H?C

### Tính Nãng
- ?? T?m ki?m theo t? khóa (tên, mô t?)
- ?? L?c theo h?nh th?c h?c (Online, Offline, Hybrid, SelfPaced)
- ?? L?c theo c?p ð? (Beginner, Intermediate, Advanced)
- ?? L?c theo kho?ng giá (min - max)
- ? Real-time search form
- ?? Responsive design

### Cách S? D?ng

#### Cho H?c Viên:
1. **Truy c?p:** `/Courses/Index`
2. **T?m ki?m:**
   - Nh?p t? khóa vào ô search
   - Ví d?: "Python", "Web Development"
3. **L?c:**
   - Ch?n h?nh th?c h?c
   - Ch?n c?p ð?
   - Nh?p m?c giá t?i thi?u/t?i ða
4. **K?t qu?:**
   - Click "T?m ki?m" ? hi?n th? k?t qu?
   - Click "Ð?t l?i" ? quay v? danh sách ð?y ð?

#### Query String Examples:
```
/Courses/Index?search=python
/Courses/Index?mode=Online&level=Beginner
/Courses/Index?minPrice=0&maxPrice=500000
/Courses/Index?search=web&mode=Online&minPrice=100000&maxPrice=1000000
```

#### API Endpoint (Backend):
```
GET /api/course?keyword=python&mode=Online&level=Beginner&minPrice=0&maxPrice=500000
  Query Parameters:
  - keyword (string) - T?m ki?m trong tiêu ð? và mô t?
  - mode (enum) - Online/Offline/Hybrid/SelfPaced
  - status (enum) - Draft/Published/Archived
  - level (enum) - Beginner/Intermediate/Advanced
  - minPrice (decimal) - Giá t?i thi?u
  - maxPrice (decimal) - Giá t?i ða

  Response:
  {
    "message": "L?y danh sách khóa h?c thành công",
    "count": 5,
    "filters": { "keyword": "python", ... },
    "courses": [...]
  }
```

### Files C?p Nh?t:
1. `JwtAuthAPI/Controllers/CourseController.cs` - Thêm tham s? search/filter
2. `CourseManagementMVC/Services/ApiService.cs` - SearchCoursesAsync() method
3. `CourseManagementMVC/Controllers/CoursesController.cs` - Index action v?i filter logic
4. `CourseManagementMVC/Views/Courses/Index.cshtml` - Search & filter UI

### Workflow:
```
User fills search form
  ?
Form submit ? GET /Courses/Index?search=X&mode=Y&...
  ?
Controller: CoursesController.Index(search, mode, level, minPrice, maxPrice)
  ?
Check: có filter params không?
  ?? YES: ApiService.SearchCoursesAsync()
  ?? NO: ApiService.GetCoursesAsync()
  ?
API Call: GET /api/course?keyword=...&mode=...
  ?
Backend Filter:
  ?? Filter keyword: title/description LIKE keyword
  ?? Filter mode: WHERE mode = @mode
  ?? Filter level: WHERE level = @level
  ?? Filter price: WHERE price >= minPrice AND price <= maxPrice
  ?? Return filtered list
  ?
Response: JSON { courses: [...] }
  ?
View: Courses/Index.cshtml renders courses + shows filter info
```

### UI Features:
- **Search box** - T?m ki?m nhanh
- **Dropdowns** - L?c h?nh th?c & c?p ð?
- **Price range** - L?c kho?ng giá
- **Search button** - Submit form
- **Reset button** - Xóa h?t filter
- **Result count** - Hi?n th? s? khóa t?m ðý?c
- **Filter chips** - Hi?n th? filter ðang ðý?c dùng
- **Empty state** - Thông báo khi không có k?t qu?

### Performance Tips:
- Ch? hi?n th? filter form cho user thý?ng (không admin)
- Backend filter t?ng trý?ng riêng bi?t (tránh case-insensitive issues)
- Cache search results n?u c?n (Redis)
- Pagination n?u quá nhi?u k?t qu?

---

## ??? Hý?ng D?n Cài Ð?t

### Backend:
```bash
# 1. T?o Migration cho LessonProgress
cd JwtAuthAPI
dotnet ef migrations add AddLessonProgress --context ApplicationDbContext

# 2. Update Database
dotnet ef database update

# 3. Build & ch?y
dotnet run
```

### Frontend:
```bash
# Build s? t? ð?ng
# Hot reload s? c?p nh?t view/controller
```

### Testing:
1. **Progress Bar:**
   - Ðãng nh?p ? Vào khóa h?c ? Study page
   - Ki?m tra progress bar hi?n th? (0% lúc ð?u)
   - Tích checkbox ? Progress update
   - Reload page ? Progress v?n ðý?c lýu

2. **Search & Filter:**
   - Vào /Courses/Index (không ph?i admin)
   - Nh?p keyword ? T?m ki?m
   - Ch?n filter ? Th? k?t h?p
   - URL query string c?p nh?t

---

## ?? Notes

### Progress Bar:
- Tính % = (Bài hoàn thành / T?ng bài) * 100
- L?n ð?u vào bài ? StartedAt = NOW()
- Ðánh d?u xong ? CompletedAt = NOW()
- Auto-create record khi click lesson l?n ð?u

### Search & Filter:
- Backend filter case-insensitive cho keyword
- Combine multiple filters v?i AND logic
- Price filter: minPrice <= price <= maxPrice
- Mode/Level là enum ? search chính xác

---

## ?? Ti?p Theo (Optional):

1. **Pagination** - Thêm phân trang cho search results
2. **Sorting** - S?p x?p theo tên, giá, rating
3. **Advanced Filters** - L?c theo instructor, rating, ngày t?o
4. **Search History** - Lýu l?ch t?m ki?m user
5. **Analytics** - Theo d?i search trends
6. **Autocomplete** - G?i ? t? khóa khi g?

---

## ? Troubleshooting

### Progress Bar không update:
- Ki?m tra Network tab ? API call status
- Ki?m tra Browser Console ? JavaScript errors
- Ki?m tra Database ? LessonProgresses table
- Ki?m tra Token ? Authorization header

### Search không ho?t ð?ng:
- Ki?m tra API URL ? /api/course query string
- Ki?m tra query params ? keyword, mode, level
- Ki?m tra Backend ? CourseController GetAll method
- Ki?m tra View ? ViewBag values

---

**Lýu ?:** C?n ch?y migration ð? create b?ng LessonProgresses trý?c khi s? d?ng tính nãng Progress Bar!
