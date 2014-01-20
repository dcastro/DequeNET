using Xunit;

namespace DequeNet.Tests.ConcurrentDeque.Internal
{
    public class PushTests
    {
        [Fact]
        public void PushRight_AppendsNode_ToEmptyDeque()
        {
            //Arrange
            const int value = 5;
            var deque = new ConcurrentDeque<int>();

            //Act
            deque.PushRight(value);

            //Assert
            var anchor = deque._anchor;
            Assert.NotNull(anchor._right);
            Assert.NotNull(anchor._left);
            Assert.Same(anchor._left, anchor._right);
            Assert.Equal(value, anchor._right._value);
        }
        
        [Fact]
        public void PushRight_AppendsNode_ToNonEmptyList()
        {
            //Arrange
            const int value = 5;
            var deque = new ConcurrentDeque<int>();
            deque.PushRight(1);

            //Act
            deque.PushRight(value);

            //Assert
            var anchor = deque._anchor;
            var rightmostNode = anchor._right;
            Assert.NotNull(rightmostNode);
            Assert.Equal(value, rightmostNode._value);
        }

        [Fact]
        public void PushRight_KeepsReference_ToPreviousRightNode()
        {
            //Arrange
            const int prevValue = 1;
            const int value = 5;
            var deque = new ConcurrentDeque<int>();
            deque.PushRight(prevValue);

            //Act
            deque.PushRight(value);

            //Assert
            var anchor = deque._anchor;
            var newNode = anchor._right;
            Assert.Equal(prevValue, newNode._left._value);
        }

        [Fact]
        public void PushRight_StabilizesDeque()
        {
            //Arrange
            const int prevValue = 1;
            const int value = 5;
            var deque = new ConcurrentDeque<int>();
            deque.PushRight(prevValue);

            //Act
            deque.PushRight(value);

            //Assert
            var anchor = deque._anchor;
            var newNode = anchor._right;
            Assert.Same(newNode, newNode._left._right);
            Assert.Equal(ConcurrentDeque<int>.DequeStatus.Stable, anchor._status);
        }

        [Fact]
        public void PushLeft_AppendsNode_ToEmptyList()
        {
            //Arrange
            const int value = 5;
            var deque = new ConcurrentDeque<int>();

            //Act
            deque.PushLeft(value);

            //Assert
            var anchor = deque._anchor;
            Assert.NotNull(anchor._right);
            Assert.NotNull(anchor._left);
            Assert.Same(anchor._left, anchor._right);
            Assert.Equal(value, anchor._left._value);
        }

        [Fact]
        public void PushLeft_AppendsNode_ToNonEmptyList()
        {
            //Arrange
            const int value = 5;
            var deque = new ConcurrentDeque<int>();
            deque.PushRight(1);

            //Act
            deque.PushLeft(value);

            //Assert
            var anchor = deque._anchor;
            var leftmostNode = anchor._left;
            Assert.NotNull(leftmostNode);
            Assert.Equal(value, leftmostNode._value);
        }

        [Fact]
        public void PushLeft_KeepsReference_ToPreviousLeftNode()
        {
            //Arrange
            const int prevValue = 1;
            const int value = 5;
            var deque = new ConcurrentDeque<int>();
            deque.PushLeft(prevValue);

            //Act
            deque.PushLeft(value);

            //Assert
            var anchor = deque._anchor;
            var newNode = anchor._left;
            Assert.Equal(prevValue, newNode._right._value);
        }

        [Fact]
        public void PushLeft_StabilizesDeque()
        {
            //Arrange
            const int prevValue = 1;
            const int value = 5;
            var deque = new ConcurrentDeque<int>();
            deque.PushLeft(prevValue);

            //Act
            deque.PushLeft(value);

            //Assert
            var anchor = deque._anchor;
            var newNode = anchor._left;
            Assert.Same(newNode, newNode._right._left);
            Assert.Equal(ConcurrentDeque<int>.DequeStatus.Stable, anchor._status);
        }
    }
}
