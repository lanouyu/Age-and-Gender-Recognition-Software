# Age-and-Gender-Recognition-Software

A software which can recognize the age and gender of the face in a photo or video, and carry out precision marketing.
* Implement the model in the provided paper and adjust parameters for a higher accuracy.
* Improve the network architecture in Caffe.
* Create a interface in Caffe for C#.
* Implement the software and UI, and introduce advertisements based on predicted age and gender.

## Existing Approach
* Based on accurate localization of facial features [1], [2].
* Neural network trained on near-frontal face images [3].
* Deep convolutional neural networks (CNN) [4].
* CNN network architecture in provided paper [5].

## Proposed Idea
* Modify on the base of [5].
* VGG model (ImageNet ILSVRC-2014), 16-layer [6].

## Experimental Results
| Classifier   | Method       |Accuracy  |
|--------------|--------------|----------|
| Age          | Best in [5]  |50.7      |
| Age          | Ours         |**69.0**  |
| Gender       | Best in [5]  |86.8      |
| Gender       | Ours         |**95.2**  |

## References
[1] Y. H. Kwon and N. da Vitoria Lobo. Age classification from facial images. In Proc. Conf. Comput. Vision Pattern Recognition, pages 762–767. IEEE, 1994. 1, 2
[2] L. Rabiner and B.-H. Juang. An introduction to hidden markov models. ASSP Magazine, IEEE, 3(1):4–16, 1986. 2
[3] B. A. Golomb, D. T. Lawrence, and T. J. Sejnowski. Sexnet: A neural network identifies sex from human faces. In Neural Inform. Process. Syst., pages 572–579, 1990. 2
[4] A. Krizhevsky, I. Sutskever, and G. E. Hinton. Imagenet classification with deep convolutional neural networks. In Neural Inform. Process. Syst., pages 1097–1105, 2012. 3, 4
[5] Gil Levi and Tal Hassner, Age and Gender Classification using Convolutional Neural Networks, IEEE Conf. on Computer Vision and Pattern Recognition (CVPR), Boston, June 2015
[6] K. Simonyan, A. Zisserman, Very Deep Convolutional Networks for Large-Scale Image Recognition. arXiv technical report, 2014
