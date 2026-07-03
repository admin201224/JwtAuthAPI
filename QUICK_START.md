# ?? QUICK START GUIDE

## ? Các Bý?c Ch?y Ngay

### 1?? Backend Setup (LessonProgress)

M? PowerShell trong folder `JwtAuthAPI`:

```powershell
# 1. T?o Migration
dotnet ef migrations add AddLessonProgress --context ApplicationDbContext

# 2. Update Database
dotnet ef database update

# 3. Build & Run
dotnet build
dotnet run
```

**K?t qu?:** B?ng `LessonProgresses` ðý?c t?o trong database

---

### 2?? Frontend Build

Visual Studio s? t? ð?ng build `CourseManagementMVC` khi có thay ð?i.

Hot reload s? c?p nh?t views mà không c?n restart.

---

### 3?? Test Tính Nãng

#### Progress Bar + Checkbox:
1. Ðãng nh?p vào app (student account)
2. Vào "Khóa h?c c?a tôi" ? Ch?n khóa h?c ? Click "Vào h?c ngay"
3. Trên trang Study:
   - Sidebar trái ? Xem progress bar (0% lúc ð?u)
   - Click vào m?t bài h?c
   - Tích checkbox "Ðánh d?u ð? hoàn thành"
   - ? Progress bar update ngay!
   - ? Icon ? xu?t hi?n trên bài trong sidebar

#### Search & Filter:
1. Ðãng nh?p (student account)
2. Vào "Khám phá khóa h?c" ? /Courses
3. ? trang Courses (không ph?i admin):
   - Nh?p t? khóa vào "T?m ki?m khóa h?c" (vd: "Python")
   - Ch?n "H?nh th?c h?c" (vd: Online)
   - Ch?n "C?p ð?" (vd: Beginner)
   - Nh?p giá t?i thi?u/t?i ða
   - Click "T?m ki?m"
   - ? Danh sách c?p nh?t ngay!
   - ? Hi?n th? "T?m th?y X khóa h?c"

---

## ?? File Chính C?n Bi?t

### Backend (JwtAuthAPI):
- `Models/LessonProgress.cs` ? Entity model
- `Services/LessonProgressService.cs` ? Logic
- `Controllers/LessonProgressController.cs` ? API
- `Controllers/CourseController.cs` ? Search logic

### Frontend (CourseManagementMVC):
- `Views/MyCourses/Study.cshtml` ? Progress UI + JavaScript
- `Views/Courses/Index.cshtml` ? Search form
- `Controllers/CoursesController.cs` ? Handle search params
- `Services/ApiService.cs` ? Call API

---

## ?? URL Shortcuts

**Student Learning:**
- http://localhost:5273/MyCourses ? Khóa h?c c?a tôi
- http://localhost:5273/MyCourses/Study/1 ? Trang h?c (v?i progress bar)

**Student Browsing:**
- http://localhost:5273/Courses ? Danh sách khóa (v?i search form)
- http://localhost:5273/Courses?search=python ? Search result

**Admin Management:**
- http://localhost:5273/Courses?mode=Online ? Ch? admin th?y (search form hidden)

---

## ??? Troubleshooting

**Progress bar không update:**
```
1. Check Network tab (F12) ? API call status
2. Check Console ? JavaScript errors
3. Check Database ? SELECT * FROM LessonProgresses
4. Check Token ? Ðãng nh?p l?i
```

**Search không ho?t ð?ng:**
```
1. Ki?m tra URL query string
2. Ki?m tra Backend log ? /api/course?keyword=...
3. Ki?m tra database ? Có course không?
4. Refresh page ? Clear cache
```

**Migration error:**
```
1. Ð?m b?o database connection string ðúng
2. Xóa folder Migrations n?u có l?i
3. Ch?y: dotnet ef database drop --force
4. Ch?y: dotnet ef database update
```

---

## ?? Database Verification

```sql
-- Check LessonProgresses table ðý?c t?o?
SELECT * FROM LessonProgresses;

-- Check có data không?
SELECT UserId, ContentId, IsCompleted, ProgressPercentage 
FROM LessonProgresses 
WHERE UserId = 1;

-- Check courses có data?
SELECT Id, Title, Price FROM Courses LIMIT 5;
```

---

## ?? Tips & Tricks

1. **Progress không c?p nh?t**: Ki?m tra token JWT c?n h?p l? không ? ðãng nh?p l?i
2. **Search tr? v? 0 k?t qu?**: Ki?m tra t? khóa có match data không (case-sensitive)
3. **Checkbox b? disabled**: Ki?m tra page load thành công, có lesson list không
4. **Filter form ? admin**: B?nh thý?ng, admin không c?n search form (ch? table)

---

## ?? Support

N?u có l?i:
1. Check IMPLEMENTATION_GUIDE_VI.md (chi ti?t)
2. Check SUMMARY_CHANGES.md (thay ð?i)
3. Check Browser Console (F12)
4. Check Backend Log (terminal)

---

**Enjoy! ??**
