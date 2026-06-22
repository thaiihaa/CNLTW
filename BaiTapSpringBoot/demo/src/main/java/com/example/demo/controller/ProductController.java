package com.example.demo.controller;

import org.springframework.stereotype.Controller;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.ResponseBody;

@Controller
public class ProductController {

    @GetMapping("/product/detail/{id}")
    @ResponseBody
    public String productDetail(@PathVariable("id") Integer id) {
        if (id == null || id <= 0) {
            return "Lỗi: Product ID không hợp lệ!";
        }
        return "Product ID = " + id;
    }

    @GetMapping("/product/category")
    @ResponseBody
    public String productCategory(@RequestParam(name = "name", required = false) String name) {
        if (name == null || name.trim().isEmpty()) {
            return "Lỗi: Category name không được để trống!";
        }
        return "Category = " + name;
    }
}
