# Considerations During Implementation

1. **Event-Driven Architecture**:  
   The system is designed around an **event-driven architecture** using a message queue (simulated by `ServiceBusReceiver`) to decouple transaction requests from their processing logic. This allows for a scalable, loosely-coupled system where messages (events) trigger the necessary actions, ensuring better performance and maintainability.

2. **Service Bus and Message Lifecycle**:  
   The message lifecycle is carefully managed with methods like:
   - **`Peek`**: To inspect and retrieve messages without removing them from the queue.
   - **`Abandon`**: To retry messages that temporarily fail processing.
   - **`Complete`**: To mark a message as successfully processed.
   - **`MoveToDeadLetter`**: To handle messages that fail repeatedly after retrying, ensuring no data is lost.

3. **Transaction Validation**:  
   The core logic for crediting and debiting transactions ensures that all transaction data is validated before making any updates. This includes checking:
   - If the bank account exists.
   - If the balance is sufficient for debit operations.
   
   This helps maintain data integrity by preventing invalid or inconsistent state changes.

4. **Polly for Retry Logic**:  
   To ensure resilience, I used **Polly**, a .NET library for handling transient failures. A **retry policy** with incremental backoff is applied to retry failed operations with delays (5, 25, and 125 seconds), making the system more fault-tolerant.

5. **Separation of Concerns**:  
   The architecture clearly separates responsibilities between:
   - **`MessageWorker`**: Handles the business logic for processing the messages.
   - **`TransactionHistoryService`**: Responsible for logging transaction details to the database.
   - **`ServiceBusReceiver`**: Simulates the communication with the message queue.
   
   This separation ensures clean and maintainable code and enhances scalability.

6. **Logging**:  
   Logging is implemented using a **console logger**, which is useful for local development and debugging. However, in a production environment, this should be extended to a distributed logging system like **Postgres**, **Redis**, or **MongoDB** to aggregate logs from different services and enhance traceability.

---

# Anything Unclear?

One area that was unclear to me during the implementation was how to **efficiently listen to and peek from the queue**. While I understood the general process of message consumption, the details around continuously listening for messages from the queue were not fully outlined in the task. I opted for a polling mechanism using `Peek` to inspect the queue periodically, followed by `Task.Delay` to simulate wait times.

While this approach works in a simplified environment, I recognize that in production, a more efficient event-driven model would be ideal. Here, the queue would push messages to the worker asynchronously, reducing the need for periodic checks and improving overall system responsiveness.

---

**Note**:  
I have taken some help from internet resources during the development of this technical test, but all the ideas and core implementation were done by myself. This includes designing the architecture, defining the workflows, and ensuring the proper integration of different components in the solution.
