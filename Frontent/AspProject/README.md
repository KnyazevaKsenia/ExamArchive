# Educational Materials Portal

A web application for managing educational materials, submitting content, and preparing for exams.

## Features

- **Materials Browser**: Browse through available educational materials
- **Add Materials**: Submit new materials with course information and file attachments
- **My Page**: Track your submitted materials
- **Exam Imitating**: Practice with sample exam questions
- **Favorites**: Save and access your favorite materials

## Project Setup

```bash
# Install dependencies
npm install

# Start development server
npm run dev

# Build for production
npm run build
```

## API Integration

The application integrates with a backend API at `https://localhost:44356` for material management.

### Adding Materials

Materials are submitted to the API endpoint:
- URL: `https://localhost:44356/materials/add-material`
- Method: POST
- Content-Type: multipart/form-data
- JSON Structure:
  ```json
  {
    "userId": "Guid",
    "course": "int",
    "subject": "string",
    "teacherName": "string",
    "semester": "int",
    "description": "string",
    "files": "IFormFileCollection"
  }
  ```

## Technologies Used

- Vue.js 2
- Axios for API requests
- Vite for build tooling
- Modern CSS with Flexbox and Grid layout
