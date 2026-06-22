package com.example.demo.controller;

import com.example.demo.model.Student;
import org.springframework.stereotype.Controller;
import org.springframework.ui.Model;
import org.springframework.web.bind.annotation.GetMapping;

@Controller
public class StudentController {

    @GetMapping("/student/info")
    public String studentInfo(Model model) {
        Student student = new Student("Nguyễn Thái Hà", 20, "Công nghệ thông tin");
        model.addAttribute("student", student);
        return "student/info";
    }
}
